using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderOverviewSrv.Dto;
using Application.Services.ProviderSrvs.ProviderOverviewSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// خلاصه‌ی ارائه‌دهنده: حسابداری، کیف پول، کارت‌های بانکی، تسویه‌ها و آمار نمایندگی‌ها
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderOverviewController : ControllerBase
    {
        private readonly IProviderOverviewService _service;
        public ProviderOverviewController(IProviderOverviewService service)
        {
            _service = service;
        }

        /// <summary>
        /// خلاصه‌ی کامل یک ارائه‌دهنده
        /// </summary>
        [HttpGet("{providerId}")]
        [ProducesResponseType(typeof(BaseResultDto<ProviderOverviewVDto>), 200)]
        public async Task<IActionResult> Get(long providerId)
        {
            return Ok(await _service.GetAsync(providerId));
        }
    }
}
