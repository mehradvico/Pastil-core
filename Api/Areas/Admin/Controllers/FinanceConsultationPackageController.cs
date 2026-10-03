using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceSrv.Dto;
using Application.Services.FinanceSrvs.FinanceSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// حسابداری پکیج‌های مشاوره‌ی آنلاین (درصد سهم سایت)
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FinanceConsultationPackageController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceConsultationPackageController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        /// <summary>
        ///  ویرایش درصد کمیسیون یک پکیج مشاوره (فقط خریدهای بعدی)
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(FinanceConsultationPackageDto dto)
        {
            var result = await _financeService.UpdateConsultationPackageCommissionAsyncDto(dto);
            return Ok(result);
        }
    }
}
