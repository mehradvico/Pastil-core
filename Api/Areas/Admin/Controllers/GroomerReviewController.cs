using Application.Common.Dto.Result;
using Application.Services.GroomerSrvs.GroomerReviewSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// نظرهای آرایشگاه (بعد از رزرو تکمیل‌شده): بررسی، تأیید/رد و پاسخ
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerReviewController : ControllerBase
    {
        private readonly IGroomerReviewService _service;
        public GroomerReviewController(IGroomerReviewService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(GroomerReviewPage), 200)]
        public async Task<IActionResult> Get([FromQuery] GroomerReviewSearchInputDto dto)
        {
            dto.OwnerUserId = null;
            return Ok(await _service.SearchManageAsync(dto));
        }

        /// <summary>تأیید (accept=true) یا رد نظر</summary>
        [HttpPut("Moderate")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Moderate(long id, bool accept) => Ok(await _service.ModerateAsync(id, accept));

        [HttpPut("Answer")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Answer(GroomerReviewAnswerDto dto) => Ok(await _service.AnswerAsync(dto.Id, dto.Answer, null));
    }

    public class GroomerReviewAnswerDto
    {
        public long Id { get; set; }
        public string Answer { get; set; }
    }
}
