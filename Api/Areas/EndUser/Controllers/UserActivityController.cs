using Application.Common.Interface;
using Application.Services.CommonSrv.UserActivitySrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// ثبت رویدادهای رفتاری کاربر (batch) از وب‌اپ/اپ موبایل
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/user-activity")]
    [ApiController]
    [Authorize]
    public class UserActivityController : ControllerBase
    {
        private readonly IUserActivityService _service;
        private readonly ICurrentUserHelper _currentUser;

        public UserActivityController(IUserActivityService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpPost("track")]
        public async Task<IActionResult> Track([FromBody] TrackActivityDto dto)
        {
            var res = await _service.TrackAsync(_currentUser.CurrentUser.UserId, dto);
            return Ok(res);
        }
    }
}
