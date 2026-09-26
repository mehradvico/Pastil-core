using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceReportSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// گزارش مالی یکپارچه
    /// </summary>
    /// <remarks>همه‌ی پول پرداخت‌شده‌ی کاربران (درگاه + کیف پول) به تفکیک منبع، بازپرداخت‌ها، سهم سایت و شریک</remarks>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FinanceReportController : ControllerBase
    {
        private readonly IFinanceReportService _service;

        public FinanceReportController(IFinanceReportService service)
        {
            _service = service;
        }

        /// <summary>خلاصه‌ی مالی کل (اختیاری: بازه‌ی تاریخ)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<FinanceReportRules.Summary>), 200)]
        public async Task<IActionResult> Get([FromQuery] FinanceReportInputDto dto)
            => Ok(await _service.GetAsync(dto));
    }
}
