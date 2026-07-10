using Application.Common.Dto.Result;
using Application.Services.LocationFields.NeighborhoodSrv.Dto;
using Application.Services.LocationFields.NeighborhoodSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت محله ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class NeighborhoodController : ControllerBase
    {
        private INeighborhoodService neighborhoodService;
        /// <summary>
        /// مدیریت محله ها
        /// </summary>
        ///
        public NeighborhoodController(INeighborhoodService neighborhoodService)
        {
            this.neighborhoodService = neighborhoodService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<NeighborhoodDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await neighborhoodService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<NeighborhoodVDto>), 200)]
        public IActionResult Get([FromQuery] NeighborhoodInputDto dto)
        {
            var searchDto = neighborhoodService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResultDto<NeighborhoodDto>), 200)]
        public async Task<IActionResult> Post(NeighborhoodDto neighborhoodDto)
        {

            var dto = await neighborhoodService.InsertAsyncDto(neighborhoodDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(NeighborhoodDto neighborhoodDto)
        {
            var dto = neighborhoodService.UpdateDto(neighborhoodDto);
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
            var dto = neighborhoodService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
