using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CommonSrv.PushInboxSrv.Dto;
using Application.Services.CommonSrv.PushInboxSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// Inbox اعلان‌های کاربر — منبع حقیقت مستقل از تحویل Push.
    /// اگر FCM یا Web Push نرسد، کاربر با بازکردن اپ اعلان‌ها را از همین‌جا می‌بیند.
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IPushInboxService _inboxService;
        private readonly ICurrentUserHelper _currentUser;

        public NotificationsController(IPushInboxService inboxService, ICurrentUserHelper currentUser)
        {
            _inboxService = inboxService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// لیست اعلان‌های کاربر جاری با صفحه‌بندی. پاسخ شامل UnreadCount کل هم هست.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(NotificationInboxSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] NotificationInboxInputDto dto)
        {
            var result = await _inboxService.SearchAsync(_currentUser.CurrentUser.UserId, dto);
            return Ok(result);
        }

        /// <summary>
        /// تعداد اعلان‌های خوانده‌نشده (برای بِج).
        /// </summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _inboxService.GetUnreadCountAsync(_currentUser.CurrentUser.UserId);
            return Ok(count);
        }

        /// <summary>
        /// خوانده‌شدن یک اعلان. Idempotent است.
        /// </summary>
        [HttpPost("{id:long}/read")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Read(long id)
        {
            var result = await _inboxService.MarkReadAsync(_currentUser.CurrentUser.UserId, id);
            return Ok(result);
        }

        /// <summary>
        /// خوانده‌شدن همه‌ی اعلان‌ها. Idempotent است.
        /// </summary>
        [HttpPost("read-all")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> ReadAll()
        {
            var result = await _inboxService.MarkAllReadAsync(_currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
