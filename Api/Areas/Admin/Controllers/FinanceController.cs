using Application.Common.Dto.Result;
using Application.Services.Accounting.FinanceSrv.Dto;
using Application.Services.Accounting.FinanceSrv.Iface;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت حسابداری ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FinanceController : ControllerBase
    {
        private IFinanceService _financeService;
        public FinanceController(IFinanceService financeService)
        {
            this._financeService = financeService;
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<FinanceVDto>), 200)]
        public IActionResult Get([FromQuery] FinanceInputDto dto)
        {
            var searchDto = _financeService.Search(dto);
            return Ok(searchDto);
        }

        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        [HttpGet("{companionId}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionFinanceDetailVDto>), 200)]
        public IActionResult Get(long companionId)
        {
            var result = _financeService.SearchCompanionDetail(companionId);
            return Ok(result);
        }
    }
}
