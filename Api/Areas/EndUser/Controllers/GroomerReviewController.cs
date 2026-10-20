using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerReviewSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// ثبت نظر و امتیاز برای آرایشگاه بعد از رزرو تکمیل‌شده
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerReviewController : ControllerBase
    {
        private readonly IGroomerReviewService _service;
        private readonly ICurrentUserHelper _currentUser;

        public GroomerReviewController(IGroomerReviewService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>نظر تا تأیید ادمین نمایش داده نمی‌شود</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GroomerReviewVDto>), 200)]
        public async Task<IActionResult> Post(GroomerReviewInputDto dto) => Ok(await _service.CreateAsync(dto, _currentUser.CurrentUser.UserId));
    }
}
