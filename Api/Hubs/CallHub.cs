using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Hubs
{
    /// <summary>
    /// سیگنالینگ WebRTC برای تماس فوری درون‌برنامه‌ای بین کاربر و نماینده - جریان صوت مستقیم و نظیر-به-نظیر
    /// برقرار می‌شود، این هاب فقط پیام‌های SDP/ICE را بین دو طرف رله می‌کند.
    /// </summary>
    [Authorize]
    public class CallHub : Hub
    {
        private const int MaximumSignalPayloadLength = 32 * 1024;
        private static readonly HashSet<string> AllowedSignalTypes = new(StringComparer.Ordinal)
        {
            "offer",
            "answer",
            "ice"
        };

        private readonly IDataBaseContext _context;
        private readonly CallSessionTracker _tracker;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<CallHub> _logger;
        private readonly CallWindowScheduler _windowScheduler;
        private readonly CallDurationRecorder _durationRecorder;

        public CallHub(IDataBaseContext context, CallSessionTracker tracker, IPushNotificationService pushNotificationService, ILogger<CallHub> logger, CallWindowScheduler windowScheduler, CallDurationRecorder durationRecorder)
        {
            _windowScheduler = windowScheduler;
            _durationRecorder = durationRecorder;
            _context = context;
            _tracker = tracker;
            _pushNotificationService = pushNotificationService;
            _logger = logger;
        }

        private static string GroupName(long reserveId) => $"call-{reserveId}";

        private long? CurrentUserId
        {
            get
            {
                var claim = Context.User?.FindFirst("UserId")?.Value;
                return long.TryParse(claim, out var id) ? id : null;
            }
        }

        public async Task JoinCall(long reserveId)
        {
            var userId = CurrentUserId;
            if (!userId.HasValue)
            {
                await Clients.Caller.SendAsync("callError", "احراز هویت نامعتبر است.");
                return;
            }

            var reserve = await _context.CompanionReserves
                .Include(r => r.Booker)
                .Include(r => r.CompanionAssistance).ThenInclude(a => a.Companion)
                .FirstOrDefaultAsync(r => r.Id == reserveId && !r.IsCancel);

            if (reserve == null)
            {
                await Clients.Caller.SendAsync("callError", "رزرو یافت نشد.");
                return;
            }

            var isBooker = reserve.BookerId == userId.Value;
            var isCompanionOwner = reserve.CompanionAssistance.Companion.OwnerId == userId.Value;
            var isAssignedStaff = reserve.CompanionAssistanceUserId.HasValue &&
                await _context.CompanionAssistanceUsers.AnyAsync(u => u.Id == reserve.CompanionAssistanceUserId.Value && u.UserId == userId.Value);

            if (!isBooker && !isCompanionOwner && !isAssignedStaff)
            {
                await Clients.Caller.SendAsync("callError", "شما دسترسی به این تماس ندارید.");
                return;
            }

            if (reserve.CallEndDate.HasValue)
            {
                await Clients.Caller.SendAsync("callError", "این تماس قبلاً پایان یافته است.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(reserveId));
            var participantCount = _tracker.Join(
                reserveId,
                Context.ConnectionId,
                userId.Value,
                reserve.BookerId,
                reserve.CompanionAssistance.Companion.Name);

            if (participantCount <= 1)
            {
                await Clients.Caller.SendAsync("waitingForPeer");

                // اگر نماینده تماس را شروع کرده (اولین نفری که وصل شده)، برای کاربر رزروکننده پوش
                // فوری ارسال می‌شود تا او هم وارد صفحه‌ی تماس شود - نماینده تماس را «می‌گیرد»، نه کاربر.
                if (!isBooker)
                {
                    try
                    {
                        await _pushNotificationService.SendPushAsync(
                            PushTypeEnum.PushInAppCallStarted,
                            reserve.BookerId,
                            token1: reserve.CompanionAssistance.Companion.Name,
                            token2: reserveId.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send in-app call start push for reserve {ReserveId}.", reserveId);
                    }
                }
                return;
            }

            if (!reserve.CallStartDate.HasValue)
            {
                reserve.CallStartDate = DateTime.Now;
                _context.CompanionReserves.Update(reserve);
                await _context.SaveChangesAsync();
            }

            // نفری که دومین بوده باید offer بسازد؛ طرف دیگر منتظر بوده و باید answer بدهد.
            await Clients.Caller.SendAsync("callConnected", true);
            await Clients.OthersInGroup(GroupName(reserveId)).SendAsync("callConnected", false);
        }

        /// <summary>
        /// تماس درون‌برنامه‌ای جلسه‌ی آنلاین (بدون رزرو). سیگنالینگ همان جریان JoinCall است؛ برای استفاده‌ی مجدد از
        /// SendSignal/EndCall و ردیاب، جلسه با کلید منفی (-sessionId) ثبت می‌شود تا با شناسه‌ی رزروها تداخل نکند.
        /// اولین نفری که وصل می‌شود (شروع‌کننده = نماینده) باعث پوش «زنگ خوردن» برای کاربر می‌شود.
        /// </summary>
        public async Task JoinSessionCall(long sessionId)
        {
            var userId = CurrentUserId;
            if (!userId.HasValue)
            {
                await Clients.Caller.SendAsync("callError", "احراز هویت نامعتبر است.");
                return;
            }

            var session = await _context.OnlineSessions
                .Include(s => s.InitiatorUser)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null || session.EndDate.HasValue || (session.ChannelId != (int)OnlineSessionChannelEnum.InAppCall && session.ChannelId != (int)OnlineSessionChannelEnum.VideoCall))
            {
                await Clients.Caller.SendAsync("callError", "تماس یافت نشد.");
                return;
            }

            if (session.InitiatorUserId != userId.Value && session.TargetUserId != userId.Value)
            {
                await Clients.Caller.SendAsync("callError", "شما دسترسی به این تماس ندارید.");
                return;
            }

            // جلسه‌ی مشاوره‌ی مدت‌دار: بعد از پایان پنجره ورود ممکن نیست؛ تماس در جریان هم با تایمر سرور بسته می‌شود
            if (session.ExpireDate.HasValue && DateTime.Now >= session.ExpireDate.Value)
            {
                await Clients.Caller.SendAsync("callError", "زمان مشاوره به پایان رسیده است.");
                return;
            }

            var key = -sessionId;
            if (session.ExpireDate.HasValue)
                _windowScheduler.Schedule(key, session.ExpireDate.Value);
            var callerName = $"{session.InitiatorUser?.FirstName} {session.InitiatorUser?.LastName}".Trim();
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(key));
            var isVideo = session.ChannelId == (int)OnlineSessionChannelEnum.VideoCall;
            var participantCount = _tracker.Join(key, Context.ConnectionId, userId.Value, session.TargetUserId, callerName, isVideo);

            if (participantCount <= 1)
            {
                await Clients.Caller.SendAsync("waitingForPeer");

                if (userId.Value == session.InitiatorUserId)
                {
                    try
                    {
                        await _pushNotificationService.SendPushAsync(
                            isVideo ? PushTypeEnum.PushOnlineSessionVideoCallStarted : PushTypeEnum.PushOnlineSessionCallStarted,
                            session.TargetUserId,
                            token1: callerName,
                            token2: sessionId.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send online session call push for session {SessionId}.", sessionId);
                    }
                }
                return;
            }

            // هر دو طرف وصل‌اند: شروع اندازه‌گیری مدت واقعی تماس (برای گزارش ادمین)
            if (_tracker.MarkConnected(key))
                await _durationRecorder.RecordConnectedAsync(sessionId);

            await Clients.Caller.SendAsync("callConnected", true);
            await Clients.OthersInGroup(GroupName(key)).SendAsync("callConnected", false);
        }

        public async Task SendSignal(long reserveId, string type, string payload)
        {
            var userId = CurrentUserId;
            if (!userId.HasValue || !_tracker.IsParticipant(reserveId, Context.ConnectionId, userId.Value))
            {
                await Clients.Caller.SendAsync("callError", "شما دسترسی به این تماس ندارید.");
                return;
            }

            if (!AllowedSignalTypes.Contains(type) ||
                string.IsNullOrWhiteSpace(payload) ||
                payload.Length > MaximumSignalPayloadLength)
            {
                await Clients.Caller.SendAsync("callError", "پیام تماس نامعتبر است.");
                return;
            }

            await Clients.OthersInGroup(GroupName(reserveId)).SendAsync("signal", type, payload);
        }

        public async Task EndCall()
        {
            var result = _tracker.Leave(Context.ConnectionId);
            if (!result.HasValue)
            {
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(result.Value.ReserveId));
            await Clients.OthersInGroup(GroupName(result.Value.ReserveId)).SendAsync("callEnded");
            await RecordSessionCallSegmentAsync(result.Value.ReserveId, result.Value.Remaining);
            await FinalizeCallAsync(result.Value.ReserveId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var result = _tracker.Leave(Context.ConnectionId);
            if (result.HasValue)
            {
                await Clients.OthersInGroup(GroupName(result.Value.ReserveId)).SendAsync("callEnded");
                await RecordSessionCallSegmentAsync(result.Value.ReserveId, result.Value.Remaining);
                await FinalizeCallAsync(result.Value.ReserveId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // تماس جلسه‌ی آنلاین (کلید منفی): وقتی کمتر از دو نفر مانده، قطعه‌ی تماس تمام شده و مدتش به CallSeconds اضافه می‌شود
        private async Task RecordSessionCallSegmentAsync(long callKey, int remaining)
        {
            if (callKey >= 0 || remaining >= 2)
                return;

            var seconds = _tracker.TakeConnectedSeconds(callKey);
            await _durationRecorder.RecordSegmentAsync(-callKey, seconds);
        }

        private async Task FinalizeCallAsync(long reserveId)
        {
            // تماس جلسه‌ی آنلاین (کلید منفی) رزرو ندارد که زمان پایانش ثبت شود
            if (reserveId < 0)
            {
                return;
            }

            try
            {
                var reserve = await _context.CompanionReserves.FirstOrDefaultAsync(r => r.Id == reserveId);
                if (reserve != null && !reserve.CallEndDate.HasValue)
                {
                    reserve.CallEndDate = DateTime.Now;
                    _context.CompanionReserves.Update(reserve);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to finalize call end time for reserve {ReserveId}.", reserveId);
            }
        }
    }
}
