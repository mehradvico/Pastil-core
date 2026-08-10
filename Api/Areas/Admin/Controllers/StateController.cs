using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.LocationFields.LocationSrv.Dto;
using Application.Services.LocationFields.NeighborhoodSrv;
using Application.Services.LocationFields.StateSrv.Dto;
using Application.Services.LocationFields.StateSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// استان ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class StateController : ControllerBase
    {
        private IStateService stateService;
        public StateController(IStateService stateService)
        {
            this.stateService = stateService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<StateDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await stateService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<StateVDto>), 200)]
        public IActionResult Get([FromQuery] StateInputDto dto)
        {
            var searchDto = stateService.Search(dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// محدوده آیتم در مپ
        /// </summary>
        /// <returns></returns> 
        [HttpGet("Boundary/{id:long}")]
        [ProducesResponseType(typeof(LocationBoundaryVDto), 200)]
        public async Task<IActionResult> GetBoundary(long id)
        {
            var dto = await stateService.FindBoundaryAsync(id);
            return Ok(dto);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<StateDto>), 200)]
        public async Task<IActionResult> Post(StateDto StateDto)
        {

            var dto = await stateService.InsertAsyncDto(StateDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(StateDto StateDto)
        {
            var dto = stateService.UpdateDto(StateDto);
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
            var dto = stateService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
