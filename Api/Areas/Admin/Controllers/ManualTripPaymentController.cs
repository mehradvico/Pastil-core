using Application.Common.Dto.Result;
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
    public class ManualTripPaymentController : ControllerBase
    {
        private ITripService _tripService;
        public ManualTripPaymentController(ITripService tripService)
        {
            _tripService = tripService;
        }
        /// <summary>
        /// پرداخت دستی آیتم
        /// </summary>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(ManualPayTripDto), 200)]
        public async Task<IActionResult> Put(ManualPayTripDto dto)
        {
            var result = await _tripService.ManualTripPaymentAsync(dto);
            return Ok(result);
        }
    }
}