using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>ساخت و مشاهده‌ی سفر پت‌رسان متصل به رزرو مربی (پانسیون آموزشی در مرکز).</summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripCoachReservationController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ICurrentUserHelper _currentUser;

        public TripCoachReservationController(ITripService tripService, ICurrentUserHelper currentUser)
        {
            _tripService = tripService;
            _currentUser = currentUser;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<TripDto>), 200)]
        public async Task<IActionResult> Post(TripCoachReservationCreateDto dto)
        {
            var result = await _tripService.CreateReservationLinkedTripForCoachAsync(dto, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }

        [HttpGet("{coachReserveId}")]
        [ProducesResponseType(typeof(BaseResultDto<TripVDto>), 200)]
        public async Task<IActionResult> Get(long coachReserveId)
        {
            var result = await _tripService.GetTripForCoachReservationAsync(coachReserveId, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }

        /// <summary>برداشتن پت‌رسانِ همراه رزرو قبل از پرداخت (فقط سفرِ منتظر پرداخت؛ سفر پرداخت‌شده از مسیر لغو سفر).</summary>
        [HttpDelete("{coachReserveId}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long coachReserveId)
        {
            var result = await _tripService.DiscardBundledTripAsync(Application.Common.Enumerable.PaymentCallbackTypeEnum.CoachReserve, coachReserveId, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
