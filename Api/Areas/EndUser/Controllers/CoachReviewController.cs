using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachReviewSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// ثبت نظر و امتیاز برای مربی بعد از رزرو تکمیل‌شده
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachReviewController : ControllerBase
    {
        private readonly ICoachReviewService _service;
        private readonly ICurrentUserHelper _currentUser;

        public CoachReviewController(ICoachReviewService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>نظر تا تأیید ادمین نمایش داده نمی‌شود</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CoachReviewVDto>), 200)]
        public async Task<IActionResult> Post(CoachReviewInputDto dto) => Ok(await _service.CreateAsync(dto, _currentUser.CurrentUser.UserId));
    }
}
