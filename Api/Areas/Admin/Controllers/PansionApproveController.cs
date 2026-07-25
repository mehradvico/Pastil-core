using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پانسیون ها
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionApproveController : ControllerBase
    {
        private readonly IPansionService _PansionService;
        public PansionApproveController(IPansionService PansionService)
        {
            this._PansionService = PansionService;
        }

        /// <summary>
        ///  احراز هویت آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(PansionApproveDto dto)
        {
            var Pansion = _PansionService.UpdatePansionApproveDto(dto);
            return Ok(Pansion);
        }
    }
}
