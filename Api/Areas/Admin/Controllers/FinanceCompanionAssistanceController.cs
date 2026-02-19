using Application.Common.Dto.Result;
using Application.Services.Accounting.FinanceSrv.Dto;
using Application.Services.Accounting.FinanceSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت حسابداری خدمات
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FinanceCompanionAssistanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceCompanionAssistanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        /// <summary>
        ///  ویرایش آیتم
        /// </summary>
        /// <returns></returns> 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(FinanceCompanionAssistanceDto dto)
        {
            var result = await _financeService.UpdateCompanionAssistanceCommissionAsyncDto(dto);
            return Ok(result);
        }
    }
}
