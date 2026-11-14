using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// کاربر جاری
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CurrentUserController : ControllerBase
    {
        private readonly ICurrentUserHelper _currentUserHelper;
        private readonly IWalletService _walletService;
        /// <summary>
        /// کاربر جاری
        /// </summary>
        ///
        public CurrentUserController(ICurrentUserHelper currentUserHelper, IWalletService walletService)
        {
            _currentUserHelper = currentUserHelper;
            _walletService = walletService;
        }
        /// <summary>
        ///  دریافت 
        /// </summary>

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<CurrentUserDto>), 200)]
        public async Task<IActionResult> Get()
        {
            var currentUser = _currentUserHelper.CurrentUser;
            if (currentUser == null)
            {
                return Ok(new BaseResultDto(false));
            }
            else
            {
                // نقد + اعتبار کلاب: بدون این، کاربری که فقط هدیه‌ی کلاب دارد موجودی صفر می‌دید و گزینه‌ی پرداخت با کیف پول خاموش بود
                var balance = await _walletService.GetDisplayBalanceAsync(currentUser.UserId);
                currentUser.WalletAmount = balance.Total;
                currentUser.WalletCashAmount = balance.Cash;
                currentUser.WalletCreditAmount = balance.Credit;
                currentUser.WalletCreditExpiresAt = balance.CreditExpiresAt;
                return Ok(new BaseResultDto<CurrentUserDto>(true, currentUser));
            }
        }
    }
}
