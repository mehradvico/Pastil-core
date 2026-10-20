using Application.Services.GroomerSrvs.GroomerReviewSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// نظرهای تأییدشده‌ی آرایشگاه (عمومی)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class GroomerReviewController : ControllerBase
    {
        private readonly IGroomerReviewService _service;
        public GroomerReviewController(IGroomerReviewService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(GroomerReviewPage), 200)]
        public async Task<IActionResult> Get([FromQuery] GroomerReviewSearchInputDto dto) => Ok(await _service.SearchPublicAsync(dto));
    }
}
