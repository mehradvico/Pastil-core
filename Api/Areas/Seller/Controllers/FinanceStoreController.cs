using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.FinanceSrvs.FinanceSrv.Iface;
using Application.Services.FinanceSrvs.FinanceStoreSrv.Dto;
using Application.Services.FinanceSrvs.FinanceStoreSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// مدیریت حسابداری فروشگاه ها
    /// </summary>
    ///
    [Area("Store")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FinanceStoreController : ControllerBase
    {
        private readonly IFinanceService _financeService;
        private readonly Application.Services.FinanceSrvs.FinanceStoreSrv.Iface.IFinanceStoreService _financeStoreService;
        private readonly ICurrentUserHelper _currentUser;

        public FinanceStoreController(IFinanceService financeService, Application.Services.FinanceSrvs.FinanceStoreSrv.Iface.IFinanceStoreService financeStoreService, ICurrentUserHelper currentUser)
        {
            _financeService = financeService;
            _financeStoreService = financeStoreService;
            _currentUser = currentUser;
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<FinanceStoreVDto>), 200)]
        public IActionResult Get([FromQuery] FinanceStoreInputDto dto)
        {
            dto.StoreId = _currentUser.CurrentUser.StoreId;
            var searchDto = _financeStoreService.Search(dto);
            return Ok(searchDto);
        }
    }
}
