using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// فعال سازی پانسیون
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionActiveController : ControllerBase
    {
        private readonly IPansionService _PansionService;
        public PansionActiveController(IPansionService PansionService)
        {
            this._PansionService = PansionService;
        }

        /// <summary>
        ///  فعال سازی آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(PansionActiveDto dto)
        {
            var Pansion = _PansionService.UpdatePansionActiveDto(dto);
            return Ok(Pansion);
        }
    }
}
