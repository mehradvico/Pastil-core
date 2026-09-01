using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// لغو سفر توسط کاربر، همراه با دلیل لغو — سفر «مختومه» می‌شود (پت‌رسان)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripUserCancelController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ICurrentUserHelper _currentUser;
        public TripUserCancelController(ITripService tripService, ICurrentUserHelper currentUser)
        {
            _tripService = tripService;
            _currentUser = currentUser;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<TripVDto>), 200)]
        public async Task<IActionResult> Put(TripUserCancelDto dto)
        {
            var result = await _tripService.CancelByUserAsync(dto, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
