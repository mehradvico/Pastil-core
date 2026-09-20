using Application.Common.Interface;
using Application.Services.Accounting.UserSrv.Dto;
using Application.Services.Accounting.UserSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// تغییر موبایل کاربر
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ChangeMobileController : ControllerBase
    {
        private readonly long _currentUserId;

        private readonly IUserService _userService;
        /// <summary>
        /// تغییر موبایل کاربر
        /// </summary>
        ///
        public ChangeMobileController(ICurrentUserHelper currentUserHelper, IUserService userService)
        {
            this._userService = userService;
            _currentUserId = currentUserHelper.CurrentUser.UserId;
        }

        /// <summary>
        /// درخواست تغییر موبایل
        /// </summary>
        [HttpPost]
        [Route("changemobilerequest")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("OtpSend")]
        public async Task<IActionResult> Post(string mobile)
        {
            var dto = new ChangeMobileDto()
            {
                Mobile = mobile,
                UserId = _currentUserId
            };
            var reset = await _userService.ChangeMobileRequestAsync(dto);
            return Ok(reset);
        }
        /// <summary>
        ///  تغییر موبایل
        /// </summary>
        [HttpPost]
        [Route("changemobile")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("OtpVerify")]
        public async Task<IActionResult> Post(ChangeMobileDto dto)
        {
            dto.UserId = _currentUserId;
            var reset = await _userService.ChangeMobileAsync(dto);
            return Ok(reset);
        }

    }
}
