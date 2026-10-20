using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerReviewSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// نظرهای آرایشگاه‌های من و پاسخ به آن‌ها
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerReviewController : ControllerBase
    {
        private readonly IGroomerReviewService _service;
        private readonly ICurrentUserHelper _currentUser;

        public GroomerReviewController(IGroomerReviewService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GroomerReviewPage), 200)]
        public async Task<IActionResult> Get([FromQuery] GroomerReviewSearchInputDto dto)
        {
            dto.OwnerUserId = _currentUser.CurrentUser.UserId;
            return Ok(await _service.SearchManageAsync(dto));
        }

        [HttpPut("Answer")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Answer(GroomerReviewOwnerAnswerDto dto) =>
            Ok(await _service.AnswerAsync(dto.Id, dto.Answer, _currentUser.CurrentUser.UserId));
    }

    public class GroomerReviewOwnerAnswerDto
    {
        public long Id { get; set; }
        public string Answer { get; set; }
    }
}
