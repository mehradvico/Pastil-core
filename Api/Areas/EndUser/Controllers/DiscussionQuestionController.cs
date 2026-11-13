using Application.Common.Dto.Result;
using Application.Services.Content.DiscussionChatSrv.Iface;
using Application.Common.Interface;
using Application.Services.Content.DiscussionQuestionSrv.Dto;
using Application.Services.Content.DiscussionQuestionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت موضوع های گفت و گو
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DiscussionQuestionController : ControllerBase
    {
        private readonly ICurrentUserHelper _currentUserHelper;
        private IDiscussionQuestionService _DiscussionQuestionService;
        private readonly IDiscussionChatService _chat;
        /// <summary>
        /// مدیریت موضوع گفت و گو
        /// </summary>
        ///
        public DiscussionQuestionController(IDiscussionQuestionService DiscussionQuestionService, ICurrentUserHelper currentUserHelper, IDiscussionChatService chat)
        {
            _chat = chat;
            _DiscussionQuestionService = DiscussionQuestionService;
            _currentUserHelper = currentUserHelper;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<DiscussionQuestionDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            // فقط پرسش تأییدشده (قبلاً هر پرسشی حتی در انتظار تأیید با شناسه خوانده می‌شد)
            var dto = await _DiscussionQuestionService.FindAsyncVDto(id, visit: false);
            return Ok(dto);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(DiscussionQuestionInputDto), 200)]
        public IActionResult Get([FromQuery] DiscussionQuestionInputDto dto)
        {
            dto.Available = true;   // فقط پرسش‌های تأییدشده؛ پیام‌های خودِ کاربر از MyStatus می‌آید
            dto.HasPendingAnswers = null;
            dto.Pending = null;
            var searchDto = _DiscussionQuestionService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<DiscussionQuestionDto>), 200)]
        public async Task<IActionResult> Post(DiscussionQuestionDto DiscussionQuestionDto)
        {
            // Active/AdminConfirm/AnswerCount کلاینت نادیده گرفته می‌شود؛ پرسش «در انتظار تأیید ادمین» ثبت می‌شود
            var dto = await _chat.SubmitQuestionAsync(_currentUserHelper.CurrentUser.UserId, DiscussionQuestionDto);
            return Ok(dto);
        }
    }
}
