using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController, Authorize]
    public class TripSchoolReservationController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ICurrentUserHelper _currentUser;
        public TripSchoolReservationController(ITripService tripService, ICurrentUserHelper currentUser) { _tripService = tripService; _currentUser = currentUser; }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<TripDto>), 200)]
        public async Task<IActionResult> Post(TripSchoolReservationCreateDto dto) => Ok(await _tripService.CreateReservationLinkedTripForSchoolAsync(dto, _currentUser.CurrentUser.UserId));

        [HttpGet("{schoolReserveId}")]
        [ProducesResponseType(typeof(BaseResultDto<TripVDto>), 200)]
        public async Task<IActionResult> Get(long schoolReserveId) => Ok(await _tripService.GetTripForSchoolReservationAsync(schoolReserveId, _currentUser.CurrentUser.UserId));
    }
}
