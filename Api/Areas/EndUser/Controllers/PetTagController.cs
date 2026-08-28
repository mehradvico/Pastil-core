using Application.Common.Interface;
using Application.Services.Accounting.PetTagSrv.Dto;
using Application.Services.Accounting.PetTagSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// اتصال کد قلاده به پت کاربر
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PetTagController : ControllerBase
    {
        private readonly IPetTagService _service;
        private readonly ICurrentUserHelper _currentUserHelper;

        public PetTagController(IPetTagService service, ICurrentUserHelper currentUserHelper)
        {
            _service = service;
            _currentUserHelper = currentUserHelper;
        }

        /// <summary>
        /// وصل‌کردن یک کد قلاده به یکی از پت‌های کاربر جاری
        /// </summary>
        [HttpPost("claim")]
        public async Task<IActionResult> Claim(PetTagClaimDto dto) =>
            Ok(await _service.ClaimAsync(dto.Code, dto.UserPetId, _currentUserHelper.CurrentUser.UserId));

        /// <summary>
        /// لیست کدهای قلاده‌ی متصل به پت‌های کاربر جاری — برای غیرفعال‌کردن پت‌هایی
        /// که از قبل کد دارند در فرم انتخاب پت هنگام اتصال کد جدید
        /// </summary>
        [HttpGet("mine")]
        public async Task<IActionResult> Mine() =>
            Ok(await _service.GetMineAsync(_currentUserHelper.CurrentUser.UserId));
    }
}
