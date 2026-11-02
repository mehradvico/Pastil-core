using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProviderSrvs.ProviderUserSrv;
using Application.Services.ProviderSrvs.ProviderUserSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// کارمندان پنل ارائه‌دهنده (ProviderUser): دعوت با موبایل و نقش، تغییر نقش، فعال/غیرفعال و حذف. فقط مالک ارائه‌دهنده.
    /// پرسنل/اپراتور مراکز جدا هستند و از ClinicUser / GroomerUser / CoachUser مدیریت می‌شوند.
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderUserController : ControllerBase
    {
        private readonly IProviderUserService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ProviderUserController(IProviderUserService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        // ProviderPanelAccessFilter درخواستِ کارمند را قبل از رسیدن به این‌جا رد می‌کند؛ این چک لایه‌ی دوم است
        private bool Acting => _currentUser.CurrentUser.IsActingForProvider;
        private long OwnerUserId => _currentUser.CurrentUser.UserId;
        private static BaseResultDto Denied() => new(false, Resource.Notification.ProviderOwnerOnlyAction);

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ProviderUserVDto>>), 200)]
        public async Task<IActionResult> Get() =>
            Acting ? Ok(new BaseResultDto<List<ProviderUserVDto>>(false, Resource.Notification.ProviderOwnerOnlyAction, null)) : Ok(await _service.ListUsersAsync(OwnerUserId));

        /// <summary>دعوت کاربر با موبایل و یک نقش؛ تا خود کاربر نپذیرد هیچ دسترسی‌ای ندارد</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ProviderUserVDto>), 200)]
        public async Task<IActionResult> Post(ProviderUserInviteDto dto) =>
            Acting ? Ok(new BaseResultDto<ProviderUserVDto>(false, Resource.Notification.ProviderOwnerOnlyAction, null)) : Ok(await _service.InviteAsync(OwnerUserId, dto));

        [HttpPut("Role")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Role(ProviderUserRoleChangeDto dto) => Acting ? Ok(Denied()) : Ok(await _service.ChangeRoleAsync(OwnerUserId, dto));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(long id, bool active) => Acting ? Ok(Denied()) : Ok(await _service.SetActiveAsync(OwnerUserId, id, active));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Acting ? Ok(Denied()) : Ok(await _service.RemoveAsync(OwnerUserId, id));
    }
}
