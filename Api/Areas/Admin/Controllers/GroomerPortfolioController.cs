using Application.Common.Dto.Result;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Dto;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// نمونه‌کارهای آرایشگاه (پست‌های اینستاگرامی): ساخت، ویرایش، حذف و پنهان‌کردن
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerPortfolioController : ControllerBase
    {
        private readonly IGroomerPortfolioService _service;
        public GroomerPortfolioController(IGroomerPortfolioService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(GroomerPortfolioPage<GroomerPortfolioPostVDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] GroomerPortfolioPostSearchInputDto dto) =>
            Ok(await _service.SearchManageAsync(dto, ownerUserId: null));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GroomerPortfolioPostVDto>), 200)]
        public async Task<IActionResult> Post(GroomerPortfolioPostInputDto dto) => Ok(await _service.CreateAsync(dto, ownerUserId: null));

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<GroomerPortfolioPostVDto>), 200)]
        public async Task<IActionResult> Put(GroomerPortfolioPostInputDto dto) => Ok(await _service.UpdateAsync(dto, ownerUserId: null));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.DeleteAsync(id, ownerUserId: null));

        /// <summary>پنهان/نمایش یک پست (پنهان‌کردن نیاز به دلیل دارد)</summary>
        [HttpPut("Moderate")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Moderate(GroomerPortfolioModerationDto dto) => Ok(await _service.ModerateAsync(dto));
    }
}
