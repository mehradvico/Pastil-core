using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Common.Security;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// کاربران
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {

        private IUserService userService;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IConfiguration _configuration;
        public UserController(IUserService user, ICurrentUserHelper currentUser, IConfiguration configuration)
        {
            this.userService = user;
            _currentUser = currentUser;
            _configuration = configuration;
        }

        private bool ActorIsSuperAdmin => _currentUser.CurrentUser?.IsSuperAdmin == true;

        private static BaseResultDto Denied() => new BaseResultDto(false, Resource.Notification.AccessDenied);

        /// <summary>
        ///  اطلاعات آیتم
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<UserDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var item = await userService.FindAsyncDto(id);
            return Ok(item);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(UserSearchDto), 200)]

        public IActionResult Get([FromQuery] UserInputDto search)
        {
            var list = userService.Search(search);
            return Ok(list);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<UserDto>), 200)]

        public async Task<IActionResult> Post(UserDto userDto)
        {
            if (!ActorIsSuperAdmin)
            {
                // فقط ادمین اصلی کاربر با نقش غیر مشتری (کارمند/مدیر/نقش تفویضی) می‌سازد؛ وگرنه هر مدیرِ دارای مجوز «کاربران»
                // می‌توانست برای خودش یک حساب با پرقدرت‌ترین نقش بسازد. شماره‌ی ادمین اصلی را هم هیچ‌کس دیگر ثبت نمی‌کند.
                if (userDto == null ||
                    userDto.RoleId != (long)RoleEnum.Customer ||
                    SuperAdminIdentity.IsSuperAdminMobile(userDto.Mobile, _configuration))
                    return Ok(new BaseResultDto<UserDto>(false, Resource.Notification.AccessDenied, null));
            }
            var dto = await userService.InsertAsyncDto(userDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(UserDto userDto)
        {
            if (userDto == null)
                return Ok(Denied());

            var existing = (await userService.FindAsyncDto(userDto.Id))?.Data;
            if (existing != null)
            {
                var targetIsSuperAdmin = SuperAdminIdentity.IsSuperAdminMobile(existing.Mobile, _configuration);
                if (targetIsSuperAdmin)
                {
                    // حساب ادمین اصلی را هیچ ادمین دیگری ویرایش نمی‌کند؛ خودش هم از این مسیر نقش، شماره‌ی موبایل و قفل حسابش را
                    // نمی‌تواند عوض کند (شماره فقط با OTP از changeMobile، تا یک نشست ربوده‌شده حساب را از دست مالک خارج نکند).
                    if (!ActorIsSuperAdmin || _currentUser.CurrentUser.UserId != existing.Id)
                        return Ok(Denied());
                    userDto.RoleId = existing.RoleId;
                    userDto.Mobile = existing.Mobile;
                    userDto.Locked = false;
                }
                else if (!ActorIsSuperAdmin)
                {
                    // تغییر نقش یک کاربر = ارتقای دسترسی؛ فقط ادمین اصلی. و شماره‌ی ادمین اصلی روی هیچ حساب دیگری ثبت نمی‌شود.
                    if (userDto.RoleId != existing.RoleId ||
                        SuperAdminIdentity.IsSuperAdminMobile(userDto.Mobile, _configuration))
                        return Ok(Denied());
                }
            }
            var dto = userService.UpdateDto(userDto);
            return Ok(dto);
        }
        /// <summary>
        ///  حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = (await userService.FindAsyncDto(id))?.Data;
            // حساب ادمین اصلی از پنل حذف نمی‌شود (نه توسط مدیرها و نه توسط خودش)
            if (existing != null && SuperAdminIdentity.IsSuperAdminMobile(existing.Mobile, _configuration))
                return Ok(Denied());
            var dto = userService.DeleteDto(id);
            return Ok(dto);
        }
    }

}
