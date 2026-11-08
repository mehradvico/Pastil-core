using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Iface;
using Application.Services.FinanceSrvs.FinanceSrv.Dto;
using Application.Services.FinanceSrvs.FinanceSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// حسابداری پانسیون ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FinancePansionController : ControllerBase
    {
        private readonly IFinanceService _financeService;
        private readonly IFinanceCompanionService _financeCompanionService;

        public FinancePansionController(IFinanceService financeService, IFinanceCompanionService financeCompanionService)
        {
            _financeService = financeService;
            _financeCompanionService = financeCompanionService;
        }

        /// <summary>
        ///  گزارش مالی یک پانسیون
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<FinancePansionVDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] FinancePansionInputDto dto)
        {
            var result = await _financeCompanionService.SearchPansionAsync(dto, ownerUserId: null);
            return Ok(result);
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
