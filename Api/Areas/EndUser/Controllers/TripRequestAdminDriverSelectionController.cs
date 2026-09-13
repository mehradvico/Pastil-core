using Application.Common.Dto.Field;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// دکمه‌ی «ثبت درخواست» بعد از پیام «راننده‌ای پیدا نشد» — از ادمین‌های پاستیل
    /// می‌خواهد که خودشان برای این سفر (که به‌صورت خودکار لغو شده) راننده انتخاب کنند.
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class TripRequestAdminDriverSelectionController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ICurrentUserHelper _currentUser;
        public TripRequestAdminDriverSelectionController(ITripService tripService, ICurrentUserHelper currentUser)
        {
            _tripService = tripService;
            _currentUser = currentUser;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<TripVDto>), 200)]
        public async Task<IActionResult> Put(Id_FieldDto dto)
        {
            var result = await _tripService.RequestAdminDriverSelectionAsync(dto.Id, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
