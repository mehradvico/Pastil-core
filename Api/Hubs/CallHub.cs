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

        /// <summary>
        /// تماس درون‌برنامه‌ای رزرو کلینیک (فقط نحوه‌ی ارائه‌ی آنلاین). سیگنالینگ همان جریان JoinCall است؛ برای استفاده‌ی مجدد از
        /// SendSignal/EndCall و ردیاب، رزرو با کلید ClinicCallRules.KeyFor(id) ثبت می‌شود تا با رزرو Companion و جلسه‌ی آنلاین تداخل نکند.
        /// کلاینت برای SendSignal همین کلید را می‌فرستد (یا SendClinicSignal با شناسه‌ی رزرو).
        /// </summary>
        public async Task JoinClinicCall(long clinicReserveId)
        {
            var userId = CurrentUserId;
            if (!userId.HasValue)
            {
                await Clients.Caller.SendAsync("callError", "احراز هویت نامعتبر است.");
                return;
            }

            var reserve = await _context.ClinicReserves
                .Include(r => r.Clinic)
                .AsTracking()
                .FirstOrDefaultAsync(r => r.Id == clinicReserveId && !r.IsCancel);
            if (reserve == null)
            {
                await Clients.Caller.SendAsync("callError", "رزرو یافت نشد.");
                return;
            }

            var isBooker = reserve.BookerId == userId.Value;
            var isClinicSide = reserve.Clinic.OwnerId == userId.Value || reserve.AssigneeUserId == userId.Value;
            if (!isBooker && !isClinicSide)
            {
                await Clients.Caller.SendAsync("callError", "شما دسترسی به این تماس ندارید.");
                return;
            }

            if (reserve.CallEndDate.HasValue)
            {
                await Clients.Caller.SendAsync("callError", "این تماس قبلاً پایان یافته است.");
                return;
            }

            if (!Application.Services.ClinicSrvs.ClinicCallRules.CanJoin(
                    reserve.Mode, reserve.IsReserved, reserve.IsCancel, reserve.OwnerDecision, reserve.DoDate, reserve.CallEndDate, DateTime.Now))
            {
                await Clients.Caller.SendAsync("callError", "تماس برای این رزرو هنوز باز نیست یا رزرو آنلاین/تأییدشده نیست.");
                return;
            }

            var key = Application.Services.ClinicSrvs.ClinicCallRules.KeyFor(reserve.Id);
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(key));
            var participantCount = _tracker.Join(key, Context.ConnectionId, userId.Value, reserve.BookerId, reserve.Clinic.Name);

            if (participantCount <= 1)
            {
                await Clients.Caller.SendAsync("waitingForPeer");

                // کلینیک/پزشک اولین نفری است که وصل شده: برای رزروکننده پوش فوری تا او هم وارد تماس شود.
                if (!isBooker)
                {
                    try
                    {
                        await _pushNotificationService.SendPushAsync(
                            PushTypeEnum.PushClinicInAppCallStarted,
                            reserve.BookerId,
                            token1: reserve.Clinic.Name,
                            token2: reserve.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send in-app call start push for clinic reserve {ReserveId}.", reserve.Id);
                    }
                }
                return;
            }

            if (!reserve.CallStartDate.HasValue)
            {
                reserve.CallStartDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            await Clients.Caller.SendAsync("callConnected", true);
            await Clients.OthersInGroup(GroupName(key)).SendAsync("callConnected", false);
        }

        /// <summary>سیگنال WebRTC برای تماس رزرو کلینیک با شناسه‌ی رزرو (همان SendSignal با کلید مخصوص).</summary>
        public Task SendClinicSignal(long clinicReserveId, string type, string payload) =>
            SendSignal(Application.Services.ClinicSrvs.ClinicCallRules.KeyFor(clinicReserveId), type, payload);

        /// <summary>پایان تماس رزرو کلینیک با شناسه‌ی رزرو (idempotent؛ بعد از بررسی دسترسی همیشه CallEndDate ثبت می‌شود).</summary>
        public async Task EndClinicReserveCall(long clinicReserveId)
        {
            if (clinicReserveId <= 0)
                return;

            var userId = CurrentUserId;
            if (!userId.HasValue)
            {
                await Clients.Caller.SendAsync("callError", "احراز هویت نامعتبر است.");
                return;
            }

            var reserve = await _context.ClinicReserves.AsNoTracking()
                .Include(r => r.Clinic)
                .FirstOrDefaultAsync(r => r.Id == clinicReserveId);
            if (reserve == null)
                return;

            if (reserve.BookerId != userId.Value && reserve.Clinic.OwnerId != userId.Value && reserve.AssigneeUserId != userId.Value)
            {
                await Clients.Caller.SendAsync("callError", "شما دسترسی به این تماس ندارید.");
                return;
            }

            var key = Application.Services.ClinicSrvs.ClinicCallRules.KeyFor(reserve.Id);
            var result = _tracker.Leave(Context.ConnectionId);
            if (result.HasValue && result.Value.ReserveId == key)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(key));
                await Clients.OthersInGroup(GroupName(key)).SendAsync("callEnded");
            }
            else
            {
                _tracker.RemoveCall(key);
                await Clients.Group(GroupName(key)).SendAsync("callEnded");
            }

            await FinalizeCallAsync(key);
        }

        /// <summary>
        /// تماس درون‌برنامه‌ای رزرو مربی (فقط نحوه‌ی ارائه‌ی آنلاین). سیگنالینگ همان جریان JoinCall است؛ برای استفاده‌ی مجدد از
        /// SendSignal/EndCall و ردیاب، رزرو با کلید CoachCallRules.KeyFor(id) ثبت می‌شود تا با رزرو Companion و جلسه‌ی آنلاین تداخل نکند.
        /// کلاینت برای SendSignal همین کلید را می‌فرستد (یا SendCoachSignal با شناسه‌ی رزرو).
        /// </summary>
        public async Task JoinCoachCall(long coachReserveId)
        {
            var userId = CurrentUserId;
            if (!userId.HasValue)
            {
                await Clients.Caller.SendAsync("callError", "احراز هویت نامعتبر است.");
                return;
            }

            var reserve = await _context.CoachReserves
                .Include(r => r.Coach)
                .AsTracking()
                .FirstOrDefaultAsync(r => r.Id == coachReserveId && !r.IsCancel);
            if (reserve == null)
            {
                await Clients.Caller.SendAsync("callError", "رزرو یافت نشد.");
                return;
            }

            var isBooker = reserve.BookerId == userId.Value;
            var isCoachSide = reserve.Coach.OwnerId == userId.Value || reserve.AssigneeUserId == userId.Value;
            if (!isBooker && !isCoachSide)
            {
                await Clients.Caller.SendAsync("callError", "شما دسترسی به این تماس ندارید.");
                return;
            }

            if (reserve.CallEndDate.HasValue)
            {
                await Clients.Caller.SendAsync("callError", "این تماس قبلاً پایان یافته است.");
                return;
            }

            if (!Application.Services.CoachSrvs.CoachCallRules.CanJoin(
                    reserve.Mode, reserve.IsReserved, reserve.IsCancel, reserve.OwnerDecision, reserve.DoDate, reserve.CallEndDate, DateTime.Now))
            {
                await Clients.Caller.SendAsync("callError", "تماس برای این رزرو هنوز باز نیست یا رزرو آنلاین/تأییدشده نیست.");
                return;
            }

            var key = Application.Services.CoachSrvs.CoachCallRules.KeyFor(reserve.Id);
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(key));
            var participantCount = _tracker.Join(key, Context.ConnectionId, userId.Value, reserve.BookerId, reserve.Coach.Name);

            if (participantCount <= 1)
            {
                await Clients.Caller.SendAsync("waitingForPeer");

                // مربی/پزشک اولین نفری است که وصل شده: برای رزروکننده پوش فوری تا او هم وارد تماس شود.
                if (!isBooker)
                {
                    try
                    {
                        await _pushNotificationService.SendPushAsync(
                            PushTypeEnum.PushCoachInAppCallStarted,
                            reserve.BookerId,
                            token1: reserve.Coach.Name,
                            token2: reserve.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send in-app call start push for coach reserve {ReserveId}.", reserve.Id);
                    }
                }
                return;
            }

            if (!reserve.CallStartDate.HasValue)
            {
                reserve.CallStartDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            await Clients.Caller.SendAsync("callConnected", true);
            await Clients.OthersInGroup(GroupName(key)).SendAsync("callConnected", false);
        }

        /// <summary>سیگنال WebRTC برای تماس رزرو مربی با شناسه‌ی رزرو (همان SendSignal با کلید مخصوص).</summary>
        public Task SendCoachSignal(long coachReserveId, string type, string payload) =>
            SendSignal(Application.Services.CoachSrvs.CoachCallRules.KeyFor(coachReserveId), type, payload);

        /// <summary>پایان تماس رزرو مربی با شناسه‌ی رزرو (idempotent؛ بعد از بررسی دسترسی همیشه CallEndDate ثبت می‌شود).</summary>
        public async Task EndCoachReserveCall(long coachReserveId)
        {
            if (coachReserveId <= 0)
                return;

            var userId = CurrentUserId;
            if (!userId.HasValue)
            {
                await Clients.Caller.SendAsync("callError", "احراز هویت نامعتبر است.");
                return;
            }

            var reserve = await _context.CoachReserves.AsNoTracking()
                .Include(r => r.Coach)
                .FirstOrDefaultAsync(r => r.Id == coachReserveId);
            if (reserve == null)
                return;

            if (reserve.BookerId != userId.Value && reserve.Coach.OwnerId != userId.Value && reserve.AssigneeUserId != userId.Value)
            {
                await Clients.Caller.SendAsync("callError", "شما دسترسی به این تماس ندارید.");
                return;
            }

            var key = Application.Services.CoachSrvs.CoachCallRules.KeyFor(reserve.Id);
            var result = _tracker.Leave(Context.ConnectionId);
            if (result.HasValue && result.Value.ReserveId == key)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(key));
                await Clients.OthersInGroup(GroupName(key)).SendAsync("callEnded");
            }
            else
            {
                _tracker.RemoveCall(key);
                await Clients.Group(GroupName(key)).SendAsync("callEnded");
            }

            await FinalizeCallAsync(key);
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

        /// <summary>
        /// پایان تماس رزرو با شناسه‌ی رزرو. EndCall فقط وقتی کار می‌کند که ردیاب درون‌حافظه‌ی سرور همین اتصال را بشناسد؛
        /// بعد از ری‌استارت/دیپلوی سرور، اتصال مجدد (شناسه‌ی اتصال جدید) یا قطعی وسط تماس ردیاب اتصال را ندارد و
        /// EndCall بی‌صدا هیچ کاری نمی‌کرد، پس CallEndDate هیچ‌وقت ثبت نمی‌شد و رزرو «در جریان» می‌ماند.
        /// این متد بر اساس شناسه‌ی رزرو (بعد از بررسی دسترسی) در هر حالت پایان تماس را ثبت می‌کند و idempotent است.
        /// </summary>
        public async Task EndReserveCall(long reserveId)
        {
            if (reserveId <= 0)
            {
                return;
            }

            var userId = CurrentUserId;
            if (!userId.HasValue)
            {
                await Clients.Caller.SendAsync("callError", "احراز هویت نامعتبر است.");
                return;
            }

            var reserve = await _context.CompanionReserves
                .AsNoTracking()
                .Include(r => r.CompanionAssistance).ThenInclude(a => a.Companion)
                .FirstOrDefaultAsync(r => r.Id == reserveId);
            if (reserve == null)
            {
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

            var result = _tracker.Leave(Context.ConnectionId);
            if (result.HasValue && result.Value.ReserveId == reserveId)
            {
                // مسیر عادی: همان رفتار EndCall
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(reserveId));
                await Clients.OthersInGroup(GroupName(reserveId)).SendAsync("callEnded");
            }
            else
            {
                // ردیاب این اتصال را نمی‌شناسد (ری‌استارت سرور / اتصال مجدد): تماس را برای هر دو طرف می‌بندیم
                _tracker.RemoveCall(reserveId);
                await Clients.Group(GroupName(reserveId)).SendAsync("callEnded");
            }

            await FinalizeCallAsync(reserveId);
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

            // رزرو کلینیک: کلید = ClinicCallKeyBase + شناسه‌ی رزرو
            if (Application.Services.ClinicSrvs.ClinicCallRules.IsClinicKey(reserveId))
            {
                try
                {
                    var clinicReserveId = Application.Services.ClinicSrvs.ClinicCallRules.ReserveIdOf(reserveId);
                    var clinicReserve = await _context.ClinicReserves.AsTracking().FirstOrDefaultAsync(r => r.Id == clinicReserveId);
                    if (clinicReserve != null && !clinicReserve.CallEndDate.HasValue)
                    {
                        clinicReserve.CallEndDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to finalize call end time for clinic call key {Key}.", reserveId);
                }
                return;
            }

            // رزرو مربی: کلید = CoachCallKeyBase + شناسه‌ی رزرو
            if (Application.Services.CoachSrvs.CoachCallRules.IsCoachKey(reserveId))
            {
                try
                {
                    var coachReserveId = Application.Services.CoachSrvs.CoachCallRules.ReserveIdOf(reserveId);
                    var coachReserve = await _context.CoachReserves.AsTracking().FirstOrDefaultAsync(r => r.Id == coachReserveId);
                    if (coachReserve != null && !coachReserve.CallEndDate.HasValue)
                    {
                        coachReserve.CallEndDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to finalize call end time for coach call key {Key}.", reserveId);
                }
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
