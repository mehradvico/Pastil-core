using Application.Common.Dto.Result;
using Application.Services.TripSrv.TripOptionSrv.Dto;
using Application.Services.TripSrv.TripOptionSrv.Iface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// سفر ها
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripChooseDriverController : ControllerBase
    {
        private readonly ITripService _assistanceService;
        public TripChooseDriverController(ITripService assistanceService)
        {
            this._assistanceService = assistanceService;
        }
        /// <summary>
        ///  انتخاب راننده برای آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(TripAdminChooseDriverDto dto)
        {
            var agency = await _assistanceService.ChooseDriverAsync(dto);
            return Ok(agency);
        }
    }
}

