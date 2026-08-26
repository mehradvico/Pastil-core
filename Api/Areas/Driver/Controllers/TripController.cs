using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Driver.Controllers
{
    /// <summary>
    /// مدیریت  سفر ها
    /// </summary>
    /// 
    [Area("Driver")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public TripController(ITripService tripService, ICurrentUserHelper currentUserHelper)
        {
            this._tripService = tripService;
            this._currentUserHelper = currentUserHelper;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(TripSearchDto), 200)]
        public IActionResult Get([FromQuery] TripInputDto dto)
        {
            dto.DriverId = _currentUserHelper.CurrentUser.DriverId;
            var search = _tripService.Search(dto);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم
        /// </summary>
        /// <param name="id">شناسه سفر</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<TripDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Trip = await _tripService.FindAsyncDto(id);
            if (Trip.IsSuccess && Trip.Data?.DriverId != _currentUserHelper.CurrentUser.DriverId)
                return Ok(new BaseResultDto<TripDto>(false, Resource.Notification.AccessDenied, default));
            return Ok(Trip);
        }
    }
}
