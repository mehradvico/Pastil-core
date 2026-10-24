using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionPanelAccessSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// آیا کاربر جاری اجازه‌ی ورود به پنل/اپ نماینده را دارد؟ (ارائه‌دهنده، مالک مرکز یا عضو فعال تیم)
    /// </summary>
    /// <remarks>همیشه HTTP 200؛ اپ نماینده بعد از ورود این را می‌پرسد و اگر canEnter=false بود کاربر را به اپ مشتری برمی‌گرداند.
    /// بقیه‌ی endpointهای ناحیه‌ی Companion برای چنین کاربری 403 می‌دهند.</remarks>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionPanelAccessController : ControllerBase
    {
        private readonly ICompanionPanelAccessService _service;
        private readonly ICurrentUserHelper _currentUser;

        public CompanionPanelAccessController(ICompanionPanelAccessService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<CompanionPanelAccessVDto>), 200)]
        public async Task<IActionResult> Get()
        {
            var current = _currentUser.CurrentUser;
            var access = await _service.GetAsync(current.UserId);
            // ادمین مثل فیلتر ناحیه‌ی Companion اجازه دارد
            if (string.Equals(current.RoleEnum, Application.Common.Enumerable.RoleEnum.Admin.ToString(), StringComparison.Ordinal))
                access.CanEnter = true;
            return Ok(new BaseResultDto<CompanionPanelAccessVDto>(true, access));
        }
    }
}
