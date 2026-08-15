using Application.Common.Enumerable.Message;
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
        public async Task<IActionResult> Post(RefreshTokenDto dto)
        {
            var reset = await userTokenService.RefreshTokenAsync(dto);
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
