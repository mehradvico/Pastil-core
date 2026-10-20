using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Dto;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// دعوت‌نامه‌های عضویت در پرسنل آرایشگاه (پذیرش یا رد توسط خود کاربر)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerStaffInviteController : ControllerBase
    {
        private readonly IGroomerProfileService _service;
        private readonly ICurrentUserHelper _currentUser;

        public GroomerStaffInviteController(IGroomerProfileService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>دعوت‌نامه‌های منتظر پاسخ من</summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<GroomerStaffInviteVDto>), 200)]
        public async Task<IActionResult> Get() => Ok(await _service.ListInvitesAsync(_currentUser.CurrentUser.UserId));

        /// <summary>پذیرفتن (accept=true) یا رد دعوت</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(long id, bool accept) => Ok(await _service.RespondInviteAsync(id, _currentUser.CurrentUser.UserId, accept));
    }
}
