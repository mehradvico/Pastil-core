using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceSrv.Dto;
using Application.Services.FinanceSrvs.FinanceSrv.Iface;
using Application.Services.FinanceSrvs.FinanceStoreSrv.Dto;
using Application.Services.FinanceSrvs.FinanceStoreSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// حسابداری فروشگاه ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FinanceStoreController : ControllerBase
    {
        private readonly IFinanceService _financeService;
        private readonly Application.Services.FinanceSrvs.FinanceStoreSrv.Iface.IFinanceStoreService _financeStoreService;

        public FinanceStoreController(IFinanceService financeService, Application.Services.FinanceSrvs.FinanceStoreSrv.Iface.IFinanceStoreService financeStoreService)
        {
            _financeService = financeService;
            _financeStoreService = financeStoreService;
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<FinanceStoreVDto>), 200)]
        public IActionResult Get([FromQuery] FinanceStoreInputDto dto)
        {
            var searchDto = _financeStoreService.Search(dto);
            return Ok(searchDto);
        }

        /// <summary>
        ///  ویرایش آیتم
        /// </summary>
        /// <returns></returns> 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(FinanceStoreDto dto)
        {
            var result = await _financeService.UpdateStoreCommissionAsyncDto(dto);
            return Ok(result);
        }
    }
}
