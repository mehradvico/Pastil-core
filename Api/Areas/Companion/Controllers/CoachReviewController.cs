using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachReviewSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// نظرهای مربی‌های من و پاسخ به آن‌ها
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachReviewController : ControllerBase
    {
        private readonly ICoachReviewService _service;
        private readonly ICurrentUserHelper _currentUser;

        public CoachReviewController(ICoachReviewService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CoachReviewPage), 200)]
        public async Task<IActionResult> Get([FromQuery] CoachReviewSearchInputDto dto)
        {
            dto.OwnerUserId = _currentUser.CurrentUser.UserId;
            return Ok(await _service.SearchManageAsync(dto));
        }

        [HttpPut("Answer")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Answer(CoachReviewOwnerAnswerDto dto) =>
            Ok(await _service.AnswerAsync(dto.Id, dto.Answer, _currentUser.CurrentUser.UserId));
    }

    public class CoachReviewOwnerAnswerDto
    {
        public long Id { get; set; }
        public string Answer { get; set; }
    }
}
