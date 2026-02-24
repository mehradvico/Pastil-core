using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت حسابداری کلینیک ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FinanceCompanionController : ControllerBase
    {
        private readonly IFinanceCompanionService _financeCompanionService;

        public FinanceCompanionController(IFinanceCompanionService financeCompanionService)
        {
            _financeCompanionService = financeCompanionService;
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<FinanceCompanionVDto>), 200)]
        public IActionResult Get([FromQuery] FinanceCompanionInputDto dto)
        {
            var searchDto = _financeCompanionService.Search(dto);
            return Ok(searchDto);
        }
    }
}
