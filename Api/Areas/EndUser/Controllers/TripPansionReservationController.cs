using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// ساخت سفر پت‌رسانِ متصل به یک رزرو پانسیون (تحویل پت هنگام ورود به هتل/مهد)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripPansionReservationController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ICurrentUserHelper _currentUser;
        public TripPansionReservationController(ITripService tripService, ICurrentUserHelper currentUser)
        {
            _tripService = tripService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<TripDto>), 200)]
        public async Task<IActionResult> Post(TripPansionReservationCreateDto dto)
        {
            var result = await _tripService.CreateReservationLinkedTripForPansionAsync(dto, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }

        /// <summary>
        /// سفرِ پت‌رسانِ متصل به یک رزرو پانسیون مشخص — برای نمایش وضعیت («فلان راننده تایید کرد» / «هنوز کسی قبول نکرده»)
        /// </summary>
        [HttpGet("{pansionReserveId}")]
        [ProducesResponseType(typeof(BaseResultDto<TripVDto>), 200)]
        public async Task<IActionResult> Get(long pansionReserveId)
        {
            var result = await _tripService.GetTripForPansionReservationAsync(pansionReserveId, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
