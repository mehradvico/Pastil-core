using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Driver.Controllers
{
    /// <summary>
    /// اعلام رسیدن راننده به مبدا (پت‌رسان)
    /// </summary>
    [Area("Driver")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripArrivedOriginController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ICurrentUserHelper _currentUser;
        public TripArrivedOriginController(ITripService tripService, ICurrentUserHelper currentUser)
        {
            _tripService = tripService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// «به مبدا رسیدم»
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<TripVDto>), 200)]
        public async Task<IActionResult> Put(long id)
        {
            var result = await _tripService.AdvanceTripProgressAsync(id, _currentUser.CurrentUser.DriverId, TripProgressStageEnum.ArrivedOrigin);
            return Ok(result);
        }
    }
}
