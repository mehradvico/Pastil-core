using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProviderSrvs.ProviderUserSrv;
using Application.Services.ProviderSrvs.ProviderUserSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// ارائه‌دهنده‌هایی که من کارمند فعال پنلشان هستم + کلیدهای دسترسی نقشم. منوی پنل فقط از روی Permissions ساخته شود
    /// (بخشی که نیست اصلاً نمایش داده نشود) و هر درخواست پنل هدر X-Provider-Id بگیرد.
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderMembershipController : ControllerBase
    {
        private readonly IProviderUserService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ProviderMembershipController(IProviderUserService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ProviderMembershipVDto>), 200)]
        public async Task<IActionResult> Get() => Ok(await _service.ListMembershipsAsync(_currentUser.CurrentUser.UserId));

        /// <summary>ترک همکاری با یک ارائه‌دهنده (id = ProviderUserId)</summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.LeaveAsync(id, _currentUser.CurrentUser.UserId));
    }
}
