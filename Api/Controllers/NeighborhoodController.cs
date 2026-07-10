using Application.Services.LocationFields.NeighborhoodSrv.Dto;
using Application.Services.LocationFields.NeighborhoodSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت محله ها
    /// </summary>
    ///
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class NeighborhoodController : ControllerBase
    {
        private INeighborhoodService _neighborhoodService;
        /// <summary>
        /// مدیریت محله ها
        /// </summary>
        ///
        public NeighborhoodController(INeighborhoodService neighborhoodService)
        {
            _neighborhoodService = neighborhoodService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(NeighborhoodInputDto), 200)]
        public IActionResult Get([FromQuery] NeighborhoodInputDto dto)
        {
            dto.Available = true;
            var searchDto = _neighborhoodService.Search(dto);
            return Ok(searchDto);
        }

    }
}
