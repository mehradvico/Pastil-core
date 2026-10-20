using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicReviewSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// ثبت نظر و امتیاز برای کلینیک بعد از رزرو تکمیل‌شده
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicReviewController : ControllerBase
    {
        private readonly IClinicReviewService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ClinicReviewController(IClinicReviewService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>نظر تا تأیید ادمین نمایش داده نمی‌شود</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReviewVDto>), 200)]
        public async Task<IActionResult> Post(ClinicReviewInputDto dto) => Ok(await _service.CreateAsync(dto, _currentUser.CurrentUser.UserId));
    }
}
