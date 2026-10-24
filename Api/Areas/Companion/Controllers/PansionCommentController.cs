using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PansionSrvs.PansionCommentSrv.Dto;
using Application.Services.PansionSrvs.PansionCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    public class PansionCommentReplyDto
    {
        public long Id { get; set; }
        public string Answer { get; set; }
    }

    /// <summary>
    /// نظرات پانسیون‌های من و پاسخ مالک
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    [Api.Filters.RequireCompanionScope]
    public class PansionCommentController : ControllerBase
    {
        private readonly IPansionCommentService _service;
        private readonly ICurrentUserHelper _currentUser;

        public PansionCommentController(IPansionCommentService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>نظرهای همه‌ی پانسیون‌های نمایندگی من (همه‌ی وضعیت‌ها)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PansionCommentSearchDto), 200)]
        public IActionResult Get([FromQuery] PansionCommentInputDto dto)
        {
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return NotFound(new Application.Common.Dto.Result.BaseResultDto(false, Resource.Notification.NothingFound));
            dto.CompanionId = _currentUser.CurrentUser.CompanionId;
            dto.UserId = null;
            dto.AllStatus = true;
            return Ok(_service.Search(dto));
        }

        /// <summary>پاسخ مالک به نظر (فقط متن پاسخ؛ تأیید/رد نظر با ادمین است)</summary>
        [HttpPut("Reply")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Reply([FromBody] PansionCommentReplyDto dto)
            => Ok(await _service.ReplyAsync(dto?.Id ?? 0, dto?.Answer, _currentUser.CurrentUser.CompanionId!.Value));
    }
}
