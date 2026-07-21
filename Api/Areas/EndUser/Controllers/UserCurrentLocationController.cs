using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.LocationFields.UserCurrentLocationSrv.Dto;
using Application.Services.LocationFields.UserCurrentLocationSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// موقعیت فعلی کاربر
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserCurrentLocationController : ControllerBase
    {
        private readonly IUserCurrentLocationService _userCurrentLocationService;
        private readonly ICurrentUserHelper _currentUserHelper;

        public UserCurrentLocationController(IUserCurrentLocationService userCurrentLocationService,ICurrentUserHelper currentUserHelper)
        {
            _userCurrentLocationService = userCurrentLocationService;
            _currentUserHelper = currentUserHelper;
        }

        /// <summary>
        /// ثبت یا به‌روزرسانی موقعیت فعلی کاربر
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<UserCurrentLocationDto>), 200)]
        public async Task<IActionResult> Post(SetUserCurrentLocationDto dto)
        {
            var result = await _userCurrentLocationService.SetAsyncDto(_currentUserHelper.CurrentUser.UserId, dto);
            return Ok(result);
        }
    }
}
