using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.CompanionSrvs.CompanionUserSrv.Dto;
using Application.Services.CompanionSrvs.CompanionUserSrv.Iface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// دریافت تلفن همراه کاربر
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionUserGetMobileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserHelper _currentUserHelper;
        /// <summary>
        /// دریافت تلفن همراه کاربر
        /// </summary>

        public CompanionUserGetMobileController(IUserService userService, ICurrentUserHelper currentUserHelper)
        {
            this._userService = userService;
            this._currentUserHelper = currentUserHelper;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        ///
        [HttpGet("{mobile}")]
        [EnableRateLimiting("AccountLookup")]
        [ProducesResponseType(typeof(BaseResultDto<UserMinVDto>), 200)]
        public IActionResult Get(string mobile)
        {
            if (!_currentUserHelper.CurrentUser.CompanionId.HasValue)
                return Forbid();
            var CompanionUser = _userService.GetUserMinByMobile(mobile);
            // برای افزودن همکار فقط نام/تصویر لازم است؛ ایمیل و کد معرفِ کاربرِ دیگر به نماینده نمی‌رسد
            if (CompanionUser.Data != null)
            {
                CompanionUser.Data.Email = null;
                CompanionUser.Data.ReferralCode = null;
            }
            return Ok(CompanionUser);
        }
    }
}
