using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderFinanceSrv.Dto;
using Application.Services.ProviderSrvs.ProviderFinanceSrv.Iface;
using Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// حسابداری ارائه‌دهندگان
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderFinanceController : ControllerBase
    {
        private readonly IProviderFinanceService _service;
        public ProviderFinanceController(IProviderFinanceService service)
        {
            _service = service;
        }

        /// <summary>
        /// لیست ارائه‌دهندگان با مانده‌ی تسویه‌نشده‌ی نمایندگی‌ها و فروشگاه‌ها
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProviderFinanceSearchVDto), 200)]
        public async Task<IActionResult> Get([FromQuery] ProviderFinanceInputDto dto)
        {
            return Ok(await _service.SearchAsync(dto));
        }

        /// <summary>
        /// جمع هر استخر، تفکیک هر نمایندگی/فروشگاه و کارت‌های بانکی یک ارائه‌دهنده
        /// </summary>
        [HttpGet("{providerId}")]
        [ProducesResponseType(typeof(BaseResultDto<ProviderFinanceDetailVDto>), 200)]
        public async Task<IActionResult> Get(long providerId)
        {
            return Ok(await _service.GetDetailAsync(providerId));
        }

        /// <summary>
        /// ریز ردیف‌های تسویه‌نشده‌ی یک استخر (kind: 1 نمایندگی‌ها، 2 فروشگاه‌ها) - پیش‌نمایش ثبت تسویه
        /// </summary>
        [HttpGet("{providerId}/pending")]
        [ProducesResponseType(typeof(BaseResultDto<ProviderPendingVDto>), 200)]
        public async Task<IActionResult> GetPending(long providerId, [FromQuery] SettlementKindEnum kind)
        {
            return Ok(await _service.GetPendingAsync(providerId, kind));
        }
    }
}
