using Application.Services.ClinicSrvs.ClinicReviewSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// نظرهای تأییدشده‌ی کلینیک (عمومی)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ClinicReviewController : ControllerBase
    {
        private readonly IClinicReviewService _service;
        public ClinicReviewController(IClinicReviewService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(ClinicReviewPage), 200)]
        public async Task<IActionResult> Get([FromQuery] ClinicReviewSearchInputDto dto) => Ok(await _service.SearchPublicAsync(dto));
    }
}
