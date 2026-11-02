using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProviderSrvs.ProviderUserSrv;
using Application.Services.ProviderSrvs.ProviderUserSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// دعوت‌نامه‌های کار در پنل یک ارائه‌دهنده (پذیرش یا رد توسط خود کاربر)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderUserInviteController : ControllerBase
    {
        private readonly IProviderUserService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ProviderUserInviteController(IProviderUserService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>دعوت‌نامه‌های منتظر پاسخ من</summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProviderUserInviteVDto>), 200)]
        public async Task<IActionResult> Get() => Ok(await _service.ListInvitesAsync(_currentUser.CurrentUser.UserId));

        /// <summary>پذیرفتن (accept=true) یا رد دعوت</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(long id, bool accept) => Ok(await _service.RespondInviteAsync(id, _currentUser.CurrentUser.UserId, accept));
    }
}
