using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// لوکیشن زنده‌ی راننده + وضعیت فعلی سفر، برای کاربر — فقط برای Polling در طول یک سفر فعال.
    /// </summary>
    [Area("EndUser")]
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
            var result = await _tripService.GetLiveForUserAsync(id, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
