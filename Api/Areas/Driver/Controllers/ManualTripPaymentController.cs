using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Driver.Controllers
{
    /// <summary>
    ///  پرداخت دستی سفر ها
    /// </summary>
    ///
    [Area("Driver")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ManualTripPaymentController : ControllerBase
    {
        private ITripService _tripService;
        private readonly ICurrentUserHelper _currentUser;
        public ManualTripPaymentController(ITripService tripService, ICurrentUserHelper currentUser)
        {
            _tripService = tripService;
            _currentUser = currentUser;
        }
        /// <summary>
        ///  پرداخت دستی سفر ها
        /// </summary>
        ///
        [HttpPut]
        [ProducesResponseType(typeof(ManualPayTripDto), 200)]
        public async Task<IActionResult> Put(ManualPayTripDto dto)
        {
            var trip = await _tripService.FindAsyncDto(dto.Id);
            if (!trip.IsSuccess || trip.Data?.DriverId != _currentUser.CurrentUser.DriverId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            var result = await _tripService.ManualTripPaymentAsync(dto);
            return Ok(result);
        }
    }
}
