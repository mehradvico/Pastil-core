using Application.Common.Dto.Result;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.CitySrv.Iface;
using Application.Services.LocationFields.LocationSrv.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Common.Controllers
{
    /// <summary>
    /// مدیریت شهرها
    /// </summary>
    ///
    [Area("Common")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private ICityService cityService;
        /// <summary>
        /// مدیریت شهر ها
        /// </summary>
        ///
        public CityController(ICityService cityService)
        {
            this.cityService = cityService;
        }

        /// <summary>
        ///  همه شهرها با استان
        /// </summary>
        /// <returns></returns> 

        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(BaseResultDto<CityVDto>), 200)]
        //[OutputCache(Duration = 100000)]
        public IActionResult Get()
        {
            var searchDto = cityService.GetAll();
            return Ok(searchDto);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<CityVDto>), 200)]
        public IActionResult Get([FromQuery] CityInputDto dto)
        {
            dto.PageSize = 50;
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

    }
}
