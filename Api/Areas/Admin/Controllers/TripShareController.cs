using Application.Common.Dto.Result;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// سهم های سفر
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripShareController : ControllerBase
    {
        private readonly ITripService _assistanceService;
        public TripShareController(ITripService assistanceService)
        {
            this._assistanceService = assistanceService;
        }
        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(TripShareDto dto)
        {
            var agency = await _assistanceService.UpdateTripShareAsync(dto);
            return Ok(agency);
        }
    }
}
