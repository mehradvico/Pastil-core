using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.UserPetSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// وضعیت هدیه‌ی ۲۰۰ هزار تومانی تکمیل پروفایل اولین پت (برای بنر و کارت کلاب در وب‌اپ و اپ).
    /// فقط خواندنی؛ ساخت هدیه موقع ذخیره‌ی پت و فعال‌سازی با PastilClubRewardRedeem است.
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PetProfileGiftController : ControllerBase
    {
        private readonly IPetProfileClubGift _service;
        private readonly ICurrentUserHelper _currentUser;

        public PetProfileGiftController(IPetProfileClubGift service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>
        /// status: 0 Unavailable، 1 NoPet، 2 Incomplete، 3 SaveToClaim، 4 Ready، 5 Activated، 6 Expired، 7 NotEligible.
        /// showBanner برای ۱ تا ۳ true است.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<PetProfileGiftStatusDto>), 200)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
            Ok(await _service.GetStatusAsync(_currentUser.CurrentUser.UserId, cancellationToken));
    }
}
