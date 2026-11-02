using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProviderSrvs.ProviderUserSrv;
using Application.Services.ProviderSrvs.ProviderUserSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// نقش‌های کارمندان پنل ارائه‌دهنده (مثلاً «مسئول کلینیک»، «حسابدار کلینیک») و دسترسی‌های هر نقش.
    /// فقط مالک ارائه‌دهنده؛ هیچ کارمندی حتی با همه‌ی دسترسی‌ها نمی‌تواند نقش بسازد یا دسترسی بدهد.
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderUserRoleController : ControllerBase
    {
        private readonly IProviderUserService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ProviderUserRoleController(IProviderUserService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        // ProviderPanelAccessFilter درخواستِ کارمند را قبل از رسیدن به این‌جا رد می‌کند؛ این چک لایه‌ی دوم است
        private bool Acting => _currentUser.CurrentUser.IsActingForProvider;
        private long OwnerUserId => _currentUser.CurrentUser.UserId;
        private static BaseResultDto Denied() => new(false, Resource.Notification.ProviderOwnerOnlyAction);

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ProviderUserRoleVDto>>), 200)]
        public async Task<IActionResult> Get() =>
            Acting ? Ok(new BaseResultDto<List<ProviderUserRoleVDto>>(false, Resource.Notification.ProviderOwnerOnlyAction, null)) : Ok(await _service.ListRolesAsync(OwnerUserId));

        /// <summary>ساخت نقش جدید</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ProviderUserRoleVDto>), 200)]
        public async Task<IActionResult> Post(ProviderUserRoleInputDto dto)
        {
            if (Acting)
                return Ok(new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderOwnerOnlyAction, null));
            if (dto != null)
                dto.Id = 0;
            return Ok(await _service.SaveRoleAsync(OwnerUserId, dto));
        }

        /// <summary>ویرایش نام/توضیح و جایگزینی کامل دسترسی‌های نقش؛ از درخواست بعدیِ کارمندانِ همین نقش اثر می‌کند</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ProviderUserRoleVDto>), 200)]
        public async Task<IActionResult> Put(ProviderUserRoleInputDto dto)
        {
            if (Acting)
                return Ok(new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderOwnerOnlyAction, null));
            if (dto == null || dto.Id <= 0)
                return Ok(new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderUserRoleNotFound, null));
            return Ok(await _service.SaveRoleAsync(OwnerUserId, dto));
        }

        /// <summary>حذف نقش (نقشی که به کارمندی داده شده حذف نمی‌شود)</summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Acting ? Ok(Denied()) : Ok(await _service.DeleteRoleAsync(OwnerUserId, id));
    }
}
