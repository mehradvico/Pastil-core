using Application.Common.Dto.Result;
using Application.Services.LocationFields.ParkSrv.Dto;
using Application.Services.LocationFields.ParkSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Common.Controllers
{
    /// <summary>
    /// مدیریت پارک ها
    /// </summary>
    ///
    [Area("Common")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ParkController : ControllerBase
    {
        private IParkService ParkService;
        /// <summary>
        /// مدیریت پارک ها
        /// </summary>
        ///
        public ParkController(IParkService ParkService)
        {
            this.ParkService = ParkService;
        }

        /// <summary>
        ///  همه پارک ها با محله ها
        /// </summary>
        /// <returns></returns> 

        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(BaseResultDto<ParkVDto>), 200)]
        public IActionResult Get()
        {
            var searchDto = ParkService.GetAll();
            return Ok(searchDto);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ParkVDto>), 200)]
        public IActionResult Get([FromQuery] ParkInputDto dto)
        {
            dto.PageSize = 50;
            var searchDto = ParkService.Search(dto);
            return Ok(searchDto);
        }

    }
}
