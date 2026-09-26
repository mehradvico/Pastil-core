using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ConsultationSrvs.ConsultationSessionSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationSessionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// «مشاوره‌های من» برای نماینده: فهرست خریدهای مشاوره‌ی کلینیک، شروع و ورود دوباره در پنجره‌ی زمانی
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationSessionController : ControllerBase
    {
        private readonly IConsultationSessionService _service;
        private readonly ICurrentUserHelper _currentUserHelper;
        private readonly IHubContext<CallHub> _callHub;
        private readonly CallSessionTracker _tracker;
        private readonly CallDurationRecorder _durationRecorder;

        public ConsultationSessionController(
            IConsultationSessionService service,
            ICurrentUserHelper currentUserHelper,
            IHubContext<CallHub> callHub,
            CallSessionTracker tracker,
            CallDurationRecorder durationRecorder)
        {
            _service = service;
            _currentUserHelper = currentUserHelper;
            _callHub = callHub;
            _tracker = tracker;
            _durationRecorder = durationRecorder;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationAgentItemVDto>>), 200)]
        public async Task<IActionResult> Get()
            => Ok(await _service.GetForAgentAsync(_currentUserHelper.CurrentUser.UserId));

        /// <summary>شروع مشاوره: پنجره‌ی ۳۰/۶۰ دقیقه‌ای از همین لحظه حساب می‌شود (فقط یک‌بار)</summary>
        [HttpPost("{purchaseId}/Start")]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationSessionInfoVDto>), 200)]
        public async Task<IActionResult> Start(long purchaseId)
            => Ok(await _service.StartAsync(_currentUserHelper.CurrentUser.UserId, purchaseId));

        /// <summary>تخصیص مشاوره‌ی شروع‌نشده به یک نماینده (فقط مالک). targetUserId=null ⇒ برداشتن تخصیص</summary>
        [HttpPost("{purchaseId}/Assign")]
        [ProducesResponseType(typeof(BaseResultDto<bool>), 200)]
        public async Task<IActionResult> Assign(long purchaseId, [FromQuery] long? targetUserId)
            => Ok(await _service.AssignAsync(_currentUserHelper.CurrentUser.UserId, purchaseId, targetUserId));

        /// <summary>ورود دوباره به مشاوره‌ی در جریان (تا پایان پنجره)</summary>
        [HttpPost("{purchaseId}/Enter")]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationSessionInfoVDto>), 200)]
        public async Task<IActionResult> Enter(long purchaseId)
            => Ok(await _service.EnterAsync(_currentUserHelper.CurrentUser.UserId, purchaseId));

        /// <summary>
        /// تکمیل مشاوره‌ی در جریان توسط نماینده‌ی شروع‌کننده (یا مالک کلینیک): وضعیت به «پایان‌یافته» می‌رود، جلسه بسته می‌شود،
        /// تماس درون‌برنامه‌ای (اگر باز است) برای هر دو طرف «callEnded» می‌گیرد و مدت واقعی تماس ثبت می‌شود.
        /// </summary>
        [HttpPost("{purchaseId}/Complete")]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationSessionInfoVDto>), 200)]
        public async Task<IActionResult> Complete(long purchaseId)
        {
            var result = await _service.CompleteAsync(_currentUserHelper.CurrentUser.UserId, purchaseId);
            if (result.IsSuccess && result.Data != null && result.Data.OnlineSessionId > 0)
            {
                // تماس جلسه با کلید منفی (-sessionId) ثبت می‌شود؛ همان منطق پایان خودکار پنجره (CallWindowScheduler)
                var callKey = -result.Data.OnlineSessionId;
                try
                {
                    await _callHub.Clients.Group($"call-{callKey}").SendAsync("callEnded");
                    var seconds = _tracker.TakeConnectedSeconds(callKey);
                    _tracker.RemoveCall(callKey);
                    await _durationRecorder.RecordSegmentAsync(result.Data.OnlineSessionId, seconds);
                }
                catch
                {
                    // بستن تماس بی‌اثر نباید تکمیلِ ثبت‌شده را خراب کند؛ job پایان پنجره هم جلسه را می‌بندد
                }
            }
            return Ok(result);
        }
    }
}
