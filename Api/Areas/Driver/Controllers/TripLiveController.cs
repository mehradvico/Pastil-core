using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Driver.Controllers
{
    /// <summary>
    /// لوکیشن زنده‌ی کاربر + وضعیت فعلی سفر، برای راننده — فقط برای Polling در طول یک سفر فعال.
    /// </summary>
    [Area("Driver")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripLiveController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ICurrentUserHelper _currentUser;
        public TripLiveController(ITripService tripService, ICurrentUserHelper currentUser)
        {
            _tripService = tripService;
            _currentUser = currentUser;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<TripLiveDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _tripService.GetLiveForDriverAsync(id, _currentUser.CurrentUser.DriverId);
            return Ok(result);
        }
    }
}
