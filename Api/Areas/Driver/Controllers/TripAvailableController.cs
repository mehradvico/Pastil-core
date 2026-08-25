using Application.Common.Dto.Result;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Api.Areas.Driver.Controllers
{
    /// <summary>
    /// سفرهای لحظه‌ایِ بدون‌راننده — قابل مرور و پذیرفتن برای هر راننده‌ای (پت‌رسان)
    /// </summary>
    [Area("Driver")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripAvailableController : ControllerBase
    {
        private readonly ITripService _tripService;
        public TripAvailableController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<TripVDto>>), 200)]
        public async Task<IActionResult> Get()
        {
            var result = await _tripService.GetAvailableTripsForDriverAsync();
            return Ok(result);
        }
    }
}
