using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.AddressSrv.Dto;
using Application.Services.Order.AddressSrv.iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت آدرس ها
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService AddressService;
        private readonly ICurrentUserHelper _currentUserHelper;

        /// <summary>
        /// مدیریت آدرس ها
        /// </summary>
        ///
        public AddressController(IAddressService AddressService, ICurrentUserHelper currentUserHelper)
        {
            this.AddressService = AddressService;
            this._currentUserHelper = currentUserHelper;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<AddressDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await AddressService.FindAsyncDto(id);
            if (role.IsSuccess && role.Data?.UserId != _currentUserHelper.CurrentUser.UserId)
                return Ok(new BaseResultDto<AddressDto>(false, Resource.Notification.AccessDenied, default!));
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(AddressSearchDto), 200)]
        public IActionResult Get([FromQuery] AddressInputDto dto)
        {
            dto.UserId = _currentUserHelper.CurrentUser.UserId;
            var searchDto = AddressService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<AddressDto>), 200)]
        public async Task<IActionResult> Post(AddressDto AddressDto)
        {
            AddressDto.UserId = _currentUserHelper.CurrentUser.UserId;
            var dto = await AddressService.InsertAsyncDto(AddressDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(AddressDto AddressDto)
        {
            AddressDto.UserId = _currentUserHelper.CurrentUser.UserId;
            var dto = AddressService.UpdateDto(AddressDto);
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
            var existing = await AddressService.FindAsyncDto(id);
            if (!existing.IsSuccess || existing.Data?.UserId != _currentUserHelper.CurrentUser.UserId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            var dto = AddressService.DeleteDto(id);
            return Ok(dto);
        }

        /// <summary>
        /// انتخاب این آدرس به‌عنوان آدرس منتخب کاربر (بقیه‌ی آدرس‌های همون کاربر خودکار از حالت منتخب خارج می‌شن)
        /// </summary>
        [HttpPut("Select/{id}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Select(long id)
        {
            var result = await AddressService.SelectAsync(id, _currentUserHelper.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
