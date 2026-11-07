using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderUserSrv;
using Application.Services.ProviderSrvs.ProviderUserSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// همکاران (کارمندان پنل) و نقش‌های یک ارائه‌دهنده از دید ادمین: همان عملیاتی که مالک در پنل خودش دارد،
    /// این‌بار به‌جای مالک و با همان قواعد امنیتی ProviderUserService (شناسه‌ی ارائه‌دهنده‌ی دیگر = «پیدا نشد»).
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderTeamController : ControllerBase
    {
        private readonly IProviderUserService _service;

        public ProviderTeamController(IProviderUserService service)
        {
            _service = service;
        }

        private async Task<(long? owner, IActionResult notFound)> OwnerAsync(long providerId)
        {
            var owner = await _service.OwnerUserIdAsync(providerId);
            return owner.HasValue
                ? (owner, null)
                : (null, Ok(new BaseResultDto(false, Resource.Notification.ProviderNotFound)));
        }

        /// <summary>
        /// همکاران ارائه‌دهنده (با نقش و وضعیت دعوت)
        /// </summary>
        [HttpGet("{providerId}/Users")]
        [ProducesResponseType(typeof(BaseResultDto<List<ProviderUserVDto>>), 200)]
        public async Task<IActionResult> Users(long providerId)
        {
            var (owner, notFound) = await OwnerAsync(providerId);
            return notFound ?? Ok(await _service.ListUsersAsync(owner.Value));
        }

        /// <summary>
        /// نقش‌های ارائه‌دهنده (اختصاصی و آماده) با دسترسی‌ها
        /// </summary>
        [HttpGet("{providerId}/Roles")]
        [ProducesResponseType(typeof(BaseResultDto<List<ProviderUserRoleVDto>>), 200)]
        public async Task<IActionResult> Roles(long providerId)
        {
            var (owner, notFound) = await OwnerAsync(providerId);
            return notFound ?? Ok(await _service.ListRolesAsync(owner.Value));
        }

        /// <summary>
        /// کاتالوگ دسترسی‌های قابل‌انتخاب برای نقش (فقط نمایندگی‌هایی که ارائه‌دهنده دارد)
        /// </summary>
        [HttpGet("{providerId}/Catalog")]
        [ProducesResponseType(typeof(BaseResultDto<List<ProviderPermissionScopeVDto>>), 200)]
        public async Task<IActionResult> Catalog(long providerId)
        {
            var (owner, notFound) = await OwnerAsync(providerId);
            return notFound ?? Ok(await _service.CatalogAsync(owner.Value));
        }

        /// <summary>
        /// ساخت یا ویرایش نقش اختصاصی ارائه‌دهنده (Id = 0 یعنی جدید)
        /// </summary>
        [HttpPost("{providerId}/Roles")]
        [ProducesResponseType(typeof(BaseResultDto<ProviderUserRoleVDto>), 200)]
        public async Task<IActionResult> SaveRole(long providerId, ProviderUserRoleInputDto dto)
        {
            var (owner, notFound) = await OwnerAsync(providerId);
            return notFound ?? Ok(await _service.SaveRoleAsync(owner.Value, dto));
        }

        /// <summary>
        /// حذف نقش اختصاصی (اگر به همکاری داده شده باشد رد می‌شود)
        /// </summary>
        [HttpDelete("{providerId}/Roles/{roleId}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> DeleteRole(long providerId, long roleId)
        {
            var (owner, notFound) = await OwnerAsync(providerId);
            return notFound ?? Ok(await _service.DeleteRoleAsync(owner.Value, roleId));
        }

        /// <summary>
        /// دعوت همکار با موبایل و نقش
        /// </summary>
        [HttpPost("{providerId}/Users")]
        [ProducesResponseType(typeof(BaseResultDto<ProviderUserVDto>), 200)]
        public async Task<IActionResult> Invite(long providerId, ProviderUserInviteDto dto)
        {
            var (owner, notFound) = await OwnerAsync(providerId);
            return notFound ?? Ok(await _service.InviteAsync(owner.Value, dto));
        }

        /// <summary>
        /// تغییر نقش همکار
        /// </summary>
        [HttpPut("{providerId}/Users/Role")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> ChangeRole(long providerId, ProviderUserRoleChangeDto dto)
        {
            var (owner, notFound) = await OwnerAsync(providerId);
            return notFound ?? Ok(await _service.ChangeRoleAsync(owner.Value, dto));
        }

        /// <summary>
        /// فعال/غیرفعال کردن همکار
        /// </summary>
        [HttpPut("{providerId}/Users/{providerUserId}/Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> SetActive(long providerId, long providerUserId, [FromQuery] bool active)
        {
            var (owner, notFound) = await OwnerAsync(providerId);
            return notFound ?? Ok(await _service.SetActiveAsync(owner.Value, providerUserId, active));
        }

        /// <summary>
        /// حذف همکار
        /// </summary>
        [HttpDelete("{providerId}/Users/{providerUserId}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Remove(long providerId, long providerUserId)
        {
            var (owner, notFound) = await OwnerAsync(providerId);
            return notFound ?? Ok(await _service.RemoveAsync(owner.Value, providerUserId));
        }
    }
}
