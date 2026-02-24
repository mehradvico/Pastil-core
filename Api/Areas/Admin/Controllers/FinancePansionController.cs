using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceSrv.Dto;
using Application.Services.FinanceSrvs.FinanceSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت حسابداری پانسیون ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FinancePansionController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinancePansionController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        /// <summary>
        ///  ویرایش آیتم
        /// </summary>
        /// <returns></returns> 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(FinancePansionDto dto)
        {
            var result = await _financeService.UpdatePansionCommissionAsyncDto(dto);
            return Ok(result);
        }
    }
}
