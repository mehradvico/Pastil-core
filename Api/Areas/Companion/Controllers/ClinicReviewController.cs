using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicReviewSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// نظرهای کلینیک‌های من و پاسخ به آن‌ها
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicReviewController : ControllerBase
    {
        private readonly IClinicReviewService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ClinicReviewController(IClinicReviewService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ClinicReviewPage), 200)]
        public async Task<IActionResult> Get([FromQuery] ClinicReviewSearchInputDto dto)
        {
            dto.OwnerUserId = _currentUser.CurrentUser.UserId;
            return Ok(await _service.SearchManageAsync(dto));
        }

        [HttpPut("Answer")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Answer(ClinicReviewOwnerAnswerDto dto) =>
            Ok(await _service.AnswerAsync(dto.Id, dto.Answer, _currentUser.CurrentUser.UserId));
    }

    public class ClinicReviewOwnerAnswerDto
    {
        public long Id { get; set; }
        public string Answer { get; set; }
    }
}
