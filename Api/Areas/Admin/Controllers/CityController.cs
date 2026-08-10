using Application.Common.Dto.Result;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.CitySrv.Iface;
using Application.Services.LocationFields.LocationSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    ///  شهرها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CityController : ControllerBase
    {
        private ICityService cityService;
        /// <summary>
        ///  شهرها
        /// </summary>
        ///
        public CityController(ICityService cityService)
        {
            this.cityService = cityService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CityDto>), 200)] 
        public async Task<IActionResult> Get(long id)
        {
            var role = await cityService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<CityVDto>), 200)]
        public IActionResult Get([FromQuery] CityInputDto dto)
        {
            var searchDto = cityService.Search(dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// محدوده در مپ
        /// </summary>
        /// <returns></returns> 
        [HttpGet("Boundary/{id:long}")]
        [ProducesResponseType(typeof(LocationBoundaryVDto), 200)]
        public async Task<IActionResult> GetBoundary(long id)
        {
            var dto = await cityService.FindBoundaryAsync(id);
            return Ok(dto);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CityDto>), 200)]
        public async Task<IActionResult> Post(CityDto cityDto)
        {

            var dto = await cityService.InsertAsyncDto(cityDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(CityDto cityDto)
        {
            var dto = cityService.UpdateDto(cityDto);
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
            var dto = cityService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
