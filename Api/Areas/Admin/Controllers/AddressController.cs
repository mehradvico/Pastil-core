using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.AddressSrv.Dto;
using Application.Services.Order.AddressSrv.iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// آدرس ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService AddressService;

        /// <summary>
        /// آدرس ها
        /// </summary>
        ///
        public AddressController(IAddressService AddressService)
        {
            this.AddressService = AddressService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<AdminAddressVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await AddressService.FindAdminAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(AdminAddressSearchDto), 200)]
        public IActionResult Get([FromQuery] AdminAddressInputDto dto)
        {
            var searchDto = AddressService.SearchAdmin(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<AddressDto>), 200)]
        public async Task<IActionResult> Post(AddressDto AddressDto)
        {
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
            var dto = AddressService.UpdateDto(AddressDto);
            return Ok(dto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = AddressService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
