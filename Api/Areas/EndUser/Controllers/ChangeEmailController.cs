using Application.Common.Interface;
using Application.Services.Accounting.UserSrv.Dto;
using Application.Services.Accounting.UserSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// تغییر ایمیل کاربر
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ChangeEmailController : ControllerBase
    {
        private readonly long _currentUserId;

        private readonly IUserService _userService;
        /// <summary>
        /// تغییر ایمیل کاربر
        /// </summary>
        ///
        public ChangeEmailController(ICurrentUserHelper currentUserHelper, IUserService userService)
        {
            this._userService = userService;
            _currentUserId = currentUserHelper.CurrentUser.UserId;
        }

        /// <summary>
        /// درخواست تغییر ایمیل
        /// </summary>
        [HttpPost]
        [Route("changeemailrequest")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("OtpSend")]
        public async Task<IActionResult> Post(string Email)
        {
            var dto = new ChangeEmailDto()
            {
                Email = Email,
                UserId = _currentUserId
            };
            var reset = await _userService.ChangeEmailRequestAsync(dto);
            return Ok(reset);
        }
        /// <summary>
        ///  تغییر ایمیل
        /// </summary>
        [HttpPost]
        [Route("changeemail")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("OtpVerify")]
        public async Task<IActionResult> Post(ChangeEmailDto dto)
        {
            dto.UserId = _currentUserId;
            var reset = await _userService.ChangeEmailAsync(dto);
            return Ok(reset);
        }

    }
}
