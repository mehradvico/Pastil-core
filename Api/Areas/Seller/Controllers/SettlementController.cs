using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Application.Services.FinanceSrvs.SettlementSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// مدیریت تسویه حساب ها
    /// </summary>
    ///
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SettlementController : ControllerBase
    {
        private ISettlementService SettlementService;
        private ICurrentUserHelper _currentuser;
        public SettlementController(ISettlementService SettlementService, ICurrentUserHelper currentuser)
        {
            this.SettlementService = SettlementService;
            this._currentuser = currentuser;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SettlementDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await SettlementService.FindAsyncDto(id);
            if (role.IsSuccess && !await SettlementService.BelongsToVenueAsync(id, Entities.Entities.SettlementKindEnum.Store, _currentuser.CurrentUser.StoreId))
                return Ok(new BaseResultDto<SettlementDto>(false, Resource.Notification.AccessDenied, default!));
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<SettlementVDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] SettlementInputDto dto)
        {
            // StoreId کاربر جاری ۰ است اگر فروشگاه نداشته باشد؛ ۰ یعنی «هیچ فروشگاهی».
            var storeId = _currentuser.CurrentUser.StoreId > 0 ? _currentuser.CurrentUser.StoreId : (long?)null;
            var searchDto = await SettlementService.SearchForVenueAsync(dto, Entities.Entities.SettlementKindEnum.Store, storeId);
            return Ok(searchDto);
        }
    }
}
