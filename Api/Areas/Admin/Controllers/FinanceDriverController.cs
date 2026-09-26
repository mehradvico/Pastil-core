using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceDriverSrv.Dto;
using Application.Services.FinanceSrvs.FinanceDriverSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// حسابداری رانندگان (کمیسیون، تعداد سفر و سهم‌ها)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FinanceDriverController : ControllerBase
    {
        private readonly IFinanceDriverService _financeDriverService;

        public FinanceDriverController(IFinanceDriverService financeDriverService)
        {
            _financeDriverService = financeDriverService;
        }

        /// <summary>
        /// لیست رانندگان با آمار سفرها
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(FinanceDriverListDto), 200)]
        public async Task<IActionResult> Get([FromQuery] FinanceDriverInputDto dto)
        {
            return Ok(await _financeDriverService.SearchAsync(dto));
        }

        /// <summary>
        /// جزئیات مالی یک راننده و سفرهای او
        /// </summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<FinanceDriverDetailVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            return Ok(await _financeDriverService.DetailAsync(id));
        }

        /// <summary>
        /// تنظیم درصد کمیسیون سایت برای یک راننده
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(FinanceDriverCommissionDto dto)
        {
            return Ok(await _financeDriverService.UpdateCommissionAsync(dto));
        }
    }
}
