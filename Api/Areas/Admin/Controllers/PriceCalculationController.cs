using Application.Common.Dto.Result;
using Application.Services.TripSrv.PriceCalculationSrv.Dto;
using Application.Services.TripSrv.PriceCalculationSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// محاسبه قیمت در ساعت ها
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PriceCalculationController : ControllerBase
    {
        private readonly IPriceCalculationService _priceCalculationService;
        public PriceCalculationController(IPriceCalculationService priceCalculationService)
        {
            this._priceCalculationService = priceCalculationService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(PriceCalculationSearchDto), 200)]
        public IActionResult Get([FromQuery] PriceCalculationInputDto dto)
        {
            var search = _priceCalculationService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه نمایندگی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PriceCalculationDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _priceCalculationService.FindAsyncDto(id);
            return Ok(agency);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PriceCalculationDto>), 200)]
        public async Task<IActionResult> Post(PriceCalculationDto dto)
        {
            var result = await _priceCalculationService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(PriceCalculationDto dto)
        {
            var agency = _priceCalculationService.UpdateDto(dto);
            return Ok(agency);
        }

        /// <summary>
        ///  حذف آیتم 
        /// </summary>  
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _priceCalculationService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
