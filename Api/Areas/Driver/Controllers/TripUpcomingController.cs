using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Api.Areas.Driver.Controllers
{
    /// <summary>
    /// سفرهای رزروشده/سرویسِ پذیرفته‌شده‌ی این راننده که هنوز شروع نشده‌اند (پیشِ‌رو)
    /// </summary>
    [Area("Driver")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripUpcomingController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ICurrentUserHelper _currentUser;
        public TripUpcomingController(ITripService tripService, ICurrentUserHelper currentUser)
        {
            _tripService = tripService;
            _currentUser = currentUser;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<TripVDto>>), 200)]
        public async Task<IActionResult> Get()
        {
            var driverId = _currentUser.CurrentUser.DriverId;
            if (driverId <= 0)
                return Ok(new BaseResultDto<List<TripVDto>>(false, Resource.Notification.AccessDenied, default!));

            var result = await _tripService.GetUpcomingTripsForDriverAsync(driverId);
            return Ok(result);
        }
    }
}
