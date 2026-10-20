using Application.Services.CoachSrvs.CoachReviewSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// نظرهای تأییدشده‌ی مربی (عمومی)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CoachReviewController : ControllerBase
    {
        private readonly ICoachReviewService _service;
        public CoachReviewController(ICoachReviewService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(CoachReviewPage), 200)]
        public async Task<IActionResult> Get([FromQuery] CoachReviewSearchInputDto dto) => Ok(await _service.SearchPublicAsync(dto));
    }
}
