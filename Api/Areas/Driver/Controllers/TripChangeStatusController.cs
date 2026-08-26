using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Driver.Controllers
{
    /// <summary>
    /// مدیریت تغییر وضعیت سفر
    /// </summary>
    ///
    [Area("Driver")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripChangeStatusController : ControllerBase
    {
        private readonly ITripService _assistanceService;
        private readonly ICurrentUserHelper _currentUser;
        public TripChangeStatusController(ITripService assistanceService, ICurrentUserHelper currentUser)
        {
            this._assistanceService = assistanceService;
            this._currentUser = currentUser;
        }
        /// <summary>
        ///  ویرایش آیتم
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(TripChangeStatusDto dto)
        {
            var trip = await _assistanceService.FindAsyncDto(dto.Id);
            if (!trip.IsSuccess || trip.Data?.DriverId != _currentUser.CurrentUser.DriverId)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            var agency = await _assistanceService.TripChangeStatusAsync(dto);
            return Ok(agency);
        }
    }
}
