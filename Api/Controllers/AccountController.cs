using Application.Common.Enumerable.Message;
using Application.Common.Dto.Result;
using Application.Services.Accounting.OtpVerifySrv.Dto;
using Application.Services.Accounting.OtpVerifySrv.Iface;
using Application.Services.Accounting.UserSrv.Dto;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.Accounting.UserTokenSrv.Dto;
using Application.Services.Accounting.UserTokenSrv.Iface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت حساب کاربری
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IUserTokenService userTokenService;
        private readonly IOtpVerifyService otpVerifyService;
        //private readonly IReminderService reminderService;

        /// <summary> 
        /// مدیریت حساب کاربری
        /// </summary>
        public AccountController(IUserService userService, IUserTokenService userTokenService, IOtpVerifyService otpVerifyService)
        {
            this.userService = userService;
            this.userTokenService = userTokenService;
            this.otpVerifyService = otpVerifyService;
            //this.reminderService = reminderService;
        }
        /// <summary>
        /// بررسی کاربر  SignUp=1,OneFactor = 2,TwoFactor = 3, Locked=4
        /// </summary>
        [HttpPost]
        [Route("userdetail")]
        [EnableRateLimiting("AccountLookup")]
        public async Task<IActionResult> Post(UserDetailDto dto)
        {

            var userDetail = await userService.UserDetail(dto);
            return Ok(userDetail);
        }
        /// <summary>
        /// نقش
        /// </summary>
        [HttpGet]
        [Route("userrole")]
        [EnableRateLimiting("AccountLookup")]
        public async Task<IActionResult> Get(string mobile)
        {
            var userrole = await userService.UserRole(mobile);
            return Ok(userrole);
        }
        /// <summary>
        /// ورود به حساب کاربری
        /// </summary>
        [HttpPost]
        [Route("signin")]
        [EnableRateLimiting("AccountSignIn")]
        public async Task<IActionResult> Post(SignInDto dto)
        {
            var signIn = await userService.SignIn(dto);
            return Ok(signIn);
        }
        /// <summary>
        /// ثبت نام حساب کاربری
        /// </summary>
        [HttpPost]
        [Route("signup")]
        [EnableRateLimiting("AccountSignIn")]
        public async Task<IActionResult> Post(SignUpDto dto)
        {
            var signUp = await userService.SignUp(dto);
            return Ok(signUp);
        }
        /// <summary>
        /// دریافت کد
        /// </summary>
        [HttpPost]
        [Route("otp")]
        [EnableRateLimiting("OtpSend")]
        public async Task<IActionResult> Post(OtpVerifyVDto dto)
        {
            dto.Type = MessageTypeEnum.Otp;
            var otp = await otpVerifyService.InsertAsyncDto(dto);
            return Ok(otp);
        }
        /// <summary>
        /// بررسی کد
        /// </summary>
        [HttpPost]
        [Route("CheckOtp")]
        [EnableRateLimiting("OtpVerify")]
        public async Task<IActionResult> CheckOtp(OtpVerifyVDto dto)
        {
            var otp = await otpVerifyService.CheckVerify(dto);
            return Ok(otp);
        }
        /// <summary>
        /// تغییر رمز عبور
        /// </summary>
        [HttpPost]
        [Route("changepassword")]
        [EnableRateLimiting("AccountRecovery")]
        public async Task<IActionResult> Post(ChangePasswordDto dto)
        {
            var signUp = await userService.ChangePassword(dto);
            return Ok(signUp);
        }
        /// <summary>
        /// فراموشی رمز عبور
        /// </summary>
        [HttpPost]
        [Route("forgetpassword")]
        [EnableRateLimiting("AccountRecovery")]
        public async Task<IActionResult> Post(ForgetPasswordDto dto)
        {
            var forget = await userService.ForgetPassword(dto);
            return Ok(forget);
        }
        /// <summary>
        /// بازنویسی رمز عبور
        /// </summary>
        [HttpPost]
        [Route("resetpassword")]
        [EnableRateLimiting("AccountRecovery")]
        public async Task<IActionResult> Post(ResetPasswordDto dto)
        {
            var reset = await userService.ResetPassword(dto);
            return Ok(reset);
        }
        /// <summary>
        /// دریافت توکن جدید
        /// </summary>
        [HttpPost]
        [Route("refreshtoken")]
        [EnableRateLimiting("RefreshToken")]
        public async Task<IActionResult> Post(RefreshTokenDto dto)
        {
            // بدنه‌ی ناقص خطای درخواست است (نه توکن نامعتبر)؛ قبلاً با NullReference به ۵۰۰ می‌رسید
            if (dto == null || string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.RefreshToken))
                return BadRequest(new BaseResultDto(false, Resource.Notification.InvalidData));

            var reset = await userTokenService.RefreshTokenAsync(dto);
            // اعتبارنامه‌ی refresh باطل/منقضی/استفاده‌شده/متعلق به نشست بسته‌شده = ۴۰۱ (قبلاً ۲۰۰ با isSuccess:false)؛ بدنه همان BaseResultDto
            // می‌ماند تا کلاینت‌هایی که بدنه را می‌خوانند بشکنند. خطای موقت سرور به ۵۰۰ می‌رود، نه اینجا.
            if (!reset.IsSuccess)
                return Unauthorized(reset);
            return Ok(reset);
        }

        /// <summary>
        /// خروج از حساب کاربری
        /// </summary>
        [HttpPost]
        [Route("signout")]
        public async Task<IActionResult> Post()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var signout = await userTokenService.SignOut(accessToken);
            return Ok(signout);
        }
    }
}
