using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// ساخت سفر پت‌رسانِ متصل به یک رزرو کلینیک (بردن پت تا کلینیک؛ فقط رزرو «حضوری در کلینیک»)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripClinicReservationController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ICurrentUserHelper _currentUser;
        public TripClinicReservationController(ITripService tripService, ICurrentUserHelper currentUser)
        {
            _tripService = tripService;
            _currentUser = currentUser;
        }

        /// <summary>ثبت سفر پت‌رسان برای رزرو کلینیک</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<TripDto>), 200)]
        public async Task<IActionResult> Post(TripClinicReservationCreateDto dto)
        {
            var result = await _tripService.CreateReservationLinkedTripForClinicAsync(dto, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }

        /// <summary>سفرِ پت‌رسانِ متصل به یک رزرو کلینیک مشخص (وضعیت راننده)</summary>
        [HttpGet("{clinicReserveId}")]
        [ProducesResponseType(typeof(BaseResultDto<TripVDto>), 200)]
        public async Task<IActionResult> Get(long clinicReserveId)
        {
            var result = await _tripService.GetTripForClinicReservationAsync(clinicReserveId, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
