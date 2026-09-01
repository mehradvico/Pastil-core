using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Driver.Controllers
{
    /// <summary>
    /// لغو سفرِ پذیرفته‌شده توسط راننده، همراه با دلیل لغو — سفر «مختومه» می‌شود (پت‌رسان)
    /// </summary>
    [Area("Driver")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripDriverCancelController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ICurrentUserHelper _currentUser;
        public TripDriverCancelController(ITripService tripService, ICurrentUserHelper currentUser)
        {
            _tripService = tripService;
            _currentUser = currentUser;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<TripVDto>), 200)]
        public async Task<IActionResult> Put(TripDriverCancelDto dto)
        {
            var driverId = _currentUser.CurrentUser.DriverId;
            if (driverId <= 0)
                return Ok(new BaseResultDto<TripVDto>(false, Resource.Notification.AccessDenied, default!));

            var result = await _tripService.CancelByDriverAsync(dto, driverId);
            return Ok(result);
        }
    }
}
