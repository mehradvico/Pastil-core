using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.PriceCalculationSrv.Dto;
using Application.Services.TripSrv.TripAddressSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت ادرس های ذخیره شده سفر
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripAddressController : ControllerBase
    {
        private readonly ITripAddressService _tripAddress;
        private readonly ICurrentUserHelper _currentUser;
        public TripAddressController(ITripAddressService tripAddress, ICurrentUserHelper currentUser)
        {
            this._tripAddress = tripAddress;
            this._currentUser = currentUser;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(TripAddressSearchDto), 200)]
        public IActionResult Get([FromQuery] TripAddressInputDto dto)
        {
            dto.UserId = _currentUser.CurrentUser.UserId;
            var search = _tripAddress.Search(dto);
            return Ok(search);
        }

        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه خدمات</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<TripAddressDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _tripAddress.FindAsyncDto(id);
            if (agency.IsSuccess && agency.Data?.UserId != _currentUser.CurrentUser.UserId)
                return Ok(new BaseResultDto<TripAddressDto>(false, Resource.Notification.AccessDenied, default));
            return Ok(agency);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<TripAddressDto>), 200)]
        public async Task<IActionResult> Post(TripAddressDto dto)
        {
            dto.UserId = _currentUser.CurrentUser.UserId;
            var result = await _tripAddress.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  حذف آیتم 
        /// </summary>  
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _tripAddress.FindAsyncDto(id);
            if (!existing.IsSuccess || existing.Data?.UserId != _currentUser.CurrentUser.UserId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            var dto = _tripAddress.DeleteDto(id);
            return Ok(dto);
        }
    }
}
