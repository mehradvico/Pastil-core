using Application.Common.Dto.Result;
using Application.Services.Content.DiscussionChatSrv.Iface;
using Application.Common.Interface;
using Application.Services.Content.DiscussionAnswerSrv.Dto;
using Application.Services.Content.DiscussionAnswerSrv.Iface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت پاسخ های گفت و گو
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DiscussionAnswerController : ControllerBase
    {
        private readonly CurrentUserDto _currentUser;
        private IDiscussionAnswerService _discussionAnswerService;
        private readonly IDiscussionChatService _chat;
        /// <summary>
        /// مدیریت پاسخ گفت و گو
        /// </summary>
        ///
        public DiscussionAnswerController(IDiscussionAnswerService discussionAnswerService, ICurrentUserHelper currentUserHelper, IDiscussionChatService chat)
        {
            _chat = chat;
            _discussionAnswerService = discussionAnswerService;
            _currentUser = currentUserHelper.CurrentUser;
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<DiscussionAnswerDto>), 200)]
        public async Task<IActionResult> Post(DiscussionAnswerDto discussionAnswerDto)
        {
            // پاسخ به پرسش یا به یک پاسخِ تأییدشده (replyToAnswerId)؛ «در انتظار تأیید ادمین» ثبت می‌شود
            var dto = await _chat.SubmitAnswerAsync(_currentUser.UserId, discussionAnswerDto);
            return Ok(dto);
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(DiscussionAnswerInputDto), 200)]
        public IActionResult Get([FromQuery] DiscussionAnswerInputDto dto)
        {
            dto.Available = true;   // فقط تأییدشده‌ها؛ UserId کلاینت نادیده گرفته می‌شود (قبلاً کاربر لاگین‌شده فقط پاسخ‌های خودش را می‌دید)
            dto.UserId = null;
            var searchDto = _discussionAnswerService.Search(dto);
            return Ok(searchDto);
        }
    }
}
