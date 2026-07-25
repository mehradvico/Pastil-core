using Application.Common.Dto.Result;
using Application.Services.TripSrv.PriceCalculationSrv.Dto;
using Application.Services.TripSrv.TripAddressSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// ادرس های ذخیره شده سفر
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripAddressController : ControllerBase
    {
        private readonly ITripAddressService _assistanceService;
        public TripAddressController(ITripAddressService assistanceService)
        {
            this._assistanceService = assistanceService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(TripAddressSearchDto), 200)]
        public IActionResult Get([FromQuery] TripAddressInputDto dto)
        {
            var search = _assistanceService.Search(dto);
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
            var agency = await _assistanceService.FindAsyncDto(id);
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
            var result = await _assistanceService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  حذف آیتم 
        /// </summary>  
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _assistanceService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
