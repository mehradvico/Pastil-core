using Application.Services.SchoolSrvs.SchoolCourseLiveSrv;
using Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Api.Hubs
{
    /// <summary>
    /// کانال زنده‌ی «پاستیل لایو» مدرسه: فقط سیگنال‌های سبک (کامنت، حضور بینندگان) - خودِ جریان
    /// تصویر مستقیماً بین کلاینت‌ها و LiveKit Cloud برقرار می‌شود، نه از این هاب.
    /// </summary>
    [Authorize]
    public class SchoolLiveHub : Hub
    {
        private readonly ISchoolCourseLiveService _liveService;
        private readonly SchoolLiveParticipantTracker _tracker;

        public SchoolLiveHub(ISchoolCourseLiveService liveService, SchoolLiveParticipantTracker tracker)
        {
            _liveService = liveService;
            _tracker = tracker;
        }

        private static string GroupName(long liveSessionId) => $"school-live-{liveSessionId}";

        private long? CurrentUserId
        {
            get
            {
                var claim = Context.User?.FindFirst("UserId")?.Value;
                return long.TryParse(claim, out var id) ? id : null;
            }
        }

        public async Task JoinAsTrainer(long liveSessionId)
        {
            var userId = CurrentUserId;
            if (!userId.HasValue || !await _liveService.IsCompanionOwnerByUserIdAsync(liveSessionId, userId.Value))
            {
                await Clients.Caller.SendAsync("liveError", "شما دسترسی به این پخش زنده ندارید.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(liveSessionId));
        }

        public async Task JoinAsViewer(long liveSessionId, long userPetId)
        {
            var userId = CurrentUserId;
            if (!userId.HasValue || !await _liveService.IsEnrolledViewerAsync(liveSessionId, userId.Value, userPetId))
            {
                await Clients.Caller.SendAsync("liveError", "شما در این دوره ثبت‌نام نکرده‌اید.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(liveSessionId));
            _tracker.Join(liveSessionId, Context.ConnectionId, userId.Value, userPetId);
            await Clients.Group(GroupName(liveSessionId)).SendAsync("participantsUpdated");
        }

        public async Task PostComment(long liveSessionId, string message)
        {
            var userId = CurrentUserId;
            if (!userId.HasValue)
            {
                await Clients.Caller.SendAsync("liveError", "احراز هویت نامعتبر است.");
                return;
            }

            var result = await _liveService.PostCommentAsync(liveSessionId, userId.Value, message);
            if (!result.IsSuccess)
            {
                await Clients.Caller.SendAsync("liveError", result.Messages?.Count > 0 ? result.Messages[0].Item1 : "ارسال کامنت ممکن نشد.");
                return;
            }

            await Clients.Group(GroupName(liveSessionId)).SendAsync("newComment", result.Data);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var result = _tracker.Leave(Context.ConnectionId);
            if (result.HasValue)
            {
                await Clients.Group(GroupName(result.Value.LiveSessionId)).SendAsync("participantsUpdated");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
