using Application.Common.Dto.Result;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// نقشه‌ی زنده‌ی سفرهای فعال پت‌رسان (برای Polling در پنل ادمین)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripLiveController : ControllerBase
    {
        private readonly ITripService _tripService;
        public TripLiveController(ITripService tripService)
        {
            _tripService = tripService;
        }

        /// <summary>
        /// همه‌ی سفرهای فعال به همراه آخرین موقعیت راننده و کاربر
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<TripAdminLiveDto>>), 200)]
        public async Task<IActionResult> Get()
        {
            var result = await _tripService.GetActiveTripsForAdminAsync();
            return Ok(result);
        }
    }
}
