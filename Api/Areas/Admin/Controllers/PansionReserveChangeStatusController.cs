using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تغییر وضعیت رزرو پانسیون
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionReserveChangeStatusController : ControllerBase
    {
        private readonly IPansionReserveService _PansionReserveService;
        public PansionReserveChangeStatusController(IPansionReserveService PansionReserveService)
        {
            this._PansionReserveService = PansionReserveService;
        }

        /// <summary>
        ///  تغییر وصعیت آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(PansionReserveStatusDto dto)
        {
            var Pansion = await _PansionReserveService.UpdatePansionReserveStatusDto(dto);
            return Ok(Pansion);
        }
    }
}
