using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>ساخت و مشاهده‌ی سفر پت‌رسان متصل به نوبت آرایشگاه.</summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripGroomerReservationController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ICurrentUserHelper _currentUser;

        public TripGroomerReservationController(ITripService tripService, ICurrentUserHelper currentUser)
        {
            _tripService = tripService;
            _currentUser = currentUser;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<TripDto>), 200)]
        public async Task<IActionResult> Post(TripGroomerReservationCreateDto dto)
        {
            var result = await _tripService.CreateReservationLinkedTripForGroomerAsync(dto, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }

        [HttpGet("{groomerReserveId}")]
        [ProducesResponseType(typeof(BaseResultDto<TripVDto>), 200)]
        public async Task<IActionResult> Get(long groomerReserveId)
        {
            var result = await _tripService.GetTripForGroomerReservationAsync(groomerReserveId, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }

        /// <summary>برداشتن پت‌رسانِ همراه رزرو قبل از پرداخت (فقط سفرِ منتظر پرداخت؛ سفر پرداخت‌شده از مسیر لغو سفر).</summary>
        [HttpDelete("{groomerReserveId}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long groomerReserveId)
        {
            var result = await _tripService.DiscardBundledTripAsync(Application.Common.Enumerable.PaymentCallbackTypeEnum.GroomerReserve, groomerReserveId, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
