using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.LocationFields.LocationSrv.Dto;
using Application.Services.LocationFields.StateSrv.Dto;
using Application.Services.LocationFields.StateSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Common.Controllers
{
    /// <summary>
    /// مدیریت استانها
    /// </summary>
    ///
    [Area("Common")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class StateController : ControllerBase
    {
        private IStateService StateService;
        /// <summary>
        /// مدیریت استانها
        /// </summary>
        ///
        public StateController(IStateService StateService)
        {
            this.StateService = StateService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<StateVDto>), 200)]
        public IActionResult Get([FromQuery] StateInputDto dto)
        {
            var searchDto = StateService.Search(dto);
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
            var dto = await StateService.FindBoundaryAsync(id);
            return Ok(dto);
        }
    }
}
