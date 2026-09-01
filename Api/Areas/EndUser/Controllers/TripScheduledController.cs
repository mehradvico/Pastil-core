using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// ساخت سفر پت‌رسانِ تاریخ‌دار (حالت سه — نه لحظه‌ای، نه متصل به رزرو کلینیک)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripScheduledController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ICurrentUserHelper _currentUser;
        public TripScheduledController(ITripService tripService, ICurrentUserHelper currentUser)
        {
            _tripService = tripService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<TripDto>), 200)]
        public async Task<IActionResult> Post(TripScheduledCreateDto dto)
        {
            var result = await _tripService.CreateScheduledTripAsync(dto, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
