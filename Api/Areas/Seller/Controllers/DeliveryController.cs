using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Services.Order.DeliverySrv.Dto;
using Application.Services.Order.DeliverySrv.iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// مدیریت حمل و نقل
    /// </summary>
    ///
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DeliveryController : ControllerBase
    {
        private IDeliveryService DeliveryService;
        private readonly ICurrentUserHelper _currentUser;

        /// <summary>
        /// مدیریت حمل و نقل
        /// </summary>
        ///
        public DeliveryController(IDeliveryService DeliveryService, ICurrentUserHelper currentUser)
        {
            this.DeliveryService = DeliveryService;
            this._currentUser = currentUser;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<DeliveryDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var storeId = _currentUser.CurrentUser?.StoreId ?? 0;
            if (storeId <= 0)
                return BadRequest(new BaseResultDto(false, "فروشگاه فعالی برای کاربر جاری یافت نشد."));

            var role = await DeliveryService.FindForStoreAsync(id, storeId);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseInputDto), 200)]
        public IActionResult Get([FromQuery] DeliveryInputDto dto)
        {
            var storeId = _currentUser.CurrentUser?.StoreId ?? 0;
            if (storeId <= 0)
                return BadRequest(new BaseResultDto(false, "فروشگاه فعالی برای کاربر جاری یافت نشد."));

            dto.StoreId = storeId;
            var searchDto = DeliveryService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<DeliveryDto>), 200)]
        public async Task<IActionResult> Post(DeliveryDto deliveryDto)
        {
            var storeId = _currentUser.CurrentUser?.StoreId ?? 0;
            if (storeId <= 0)
                return BadRequest(new BaseResultDto(false, "فروشگاه فعالی برای کاربر جاری یافت نشد."));

            var dto = await DeliveryService.InsertForStoreAsync(deliveryDto, storeId);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(DeliveryDto DeliveryDto)
        {
            var storeId = _currentUser.CurrentUser?.StoreId ?? 0;
            if (storeId <= 0)
                return BadRequest(new BaseResultDto(false, "فروشگاه فعالی برای کاربر جاری یافت نشد."));

            var dto = await DeliveryService.UpdateForStoreAsync(DeliveryDto, storeId);
            return Ok(dto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var storeId = _currentUser.CurrentUser?.StoreId ?? 0;
            if (storeId <= 0)
                return BadRequest(new BaseResultDto(false, "فروشگاه فعالی برای کاربر جاری یافت نشد."));

            var dto = await DeliveryService.DeleteForStoreAsync(id, storeId);
            return Ok(dto);
        }
    }
}
