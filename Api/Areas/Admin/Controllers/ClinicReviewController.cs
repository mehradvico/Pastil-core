using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicReviewSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// نظرهای کلینیک (بعد از رزرو تکمیل‌شده): بررسی، تأیید/رد و پاسخ
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicReviewController : ControllerBase
    {
        private readonly IClinicReviewService _service;
        public ClinicReviewController(IClinicReviewService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(ClinicReviewPage), 200)]
        public async Task<IActionResult> Get([FromQuery] ClinicReviewSearchInputDto dto)
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
        public async Task<IActionResult> Answer(ClinicReviewAnswerDto dto) => Ok(await _service.AnswerAsync(dto.Id, dto.Answer, null));
    }

    public class ClinicReviewAnswerDto
    {
        public long Id { get; set; }
        public string Answer { get; set; }
    }
}
