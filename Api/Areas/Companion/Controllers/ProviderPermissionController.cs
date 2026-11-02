using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProviderSrvs.ProviderUserSrv;
using Application.Services.ProviderSrvs.ProviderUserSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// کاتالوگ دسترسی‌های پنل ارائه‌دهنده برای فرم ساخت نقش: نمایندگی (دسترسی مادر) ← بخش ← مشاهده/مدیریت.
    /// فقط نمایندگی‌هایی برمی‌گردند که ارائه‌دهنده دارد. فقط مالک ارائه‌دهنده (کارمند پنل هرگز).
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderPermissionController : ControllerBase
    {
        private readonly IProviderUserService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ProviderPermissionController(IProviderUserService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ProviderPermissionScopeVDto>>), 200)]
        public async Task<IActionResult> Get()
        {
            // ProviderPanelAccessFilter درخواستِ کارمند را قبل از رسیدن به این‌جا رد می‌کند؛ این چک لایه‌ی دوم است
            if (_currentUser.CurrentUser.IsActingForProvider)
                return Ok(new BaseResultDto<List<ProviderPermissionScopeVDto>>(false, Resource.Notification.ProviderOwnerOnlyAction, null));
            return Ok(await _service.CatalogAsync(_currentUser.CurrentUser.UserId));
        }
    }
}
