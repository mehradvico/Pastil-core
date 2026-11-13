using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Content.DiscussionChatSrv.Dto;
using Application.Services.Content.DiscussionChatSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// پرسش و پاسخ کالا (چت‌روم): وضعیت پیام‌های من، ویرایش و حذف پیامِ در انتظار تأیید
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DiscussionChatController : ControllerBase
    {
        private readonly IDiscussionChatService _chat;
        private readonly ICurrentUserHelper _currentUser;

        /// <summary>
        /// پرسش و پاسخ کالا (چت‌روم)
        /// </summary>
        public DiscussionChatController(IDiscussionChatService chat, ICurrentUserHelper currentUser)
        {
            _chat = chat;
            _currentUser = currentUser;
        }

        /// <summary>
        /// پیام‌های در انتظار تأیید/ردشده‌ی اخیرِ خودِ کاربر برای یک کالا + اینکه الان می‌تواند پیام جدید بفرستد یا نه (hasPending)
        /// </summary>
        [HttpGet("MyStatus")]
        [ProducesResponseType(typeof(DiscussionMyStatusVDto), 200)]
        public async Task<IActionResult> MyStatus([FromQuery] long productId) =>
            Ok(await _chat.MyStatusAsync(_currentUser.CurrentUser.UserId, productId));

        /// <summary>
        /// ویرایش پیامِ خودِ کاربر تا وقتی ادمین بررسی نکرده (kind: 1 پرسش، 2 پاسخ)
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(DiscussionChatEditDto dto) =>
            Ok(await _chat.EditAsync(_currentUser.CurrentUser.UserId, dto));

        /// <summary>
        /// حذف پیامِ خودِ کاربر تا وقتی ادمین بررسی نکرده (kind: 1 پرسش، 2 پاسخ)
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete([FromQuery] DiscussionChatKind kind, [FromQuery] long id) =>
            Ok(await _chat.DeleteAsync(_currentUser.CurrentUser.UserId, kind, id));
    }
}
