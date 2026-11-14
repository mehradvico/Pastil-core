using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت مقدار کیف پول
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletAmountController : ControllerBase
    {
        private readonly IWalletService WalletService;
        private readonly ICurrentUserHelper _currentUserHelper;

        /// <summary>
        /// مدیریت مقدار کیف پول
        /// </summary>
        ///
        public WalletAmountController(IWalletService WalletService, ICurrentUserHelper currentUserHelper)
        {
            this.WalletService = WalletService;
            this._currentUserHelper = currentUserHelper;
        }
        /// <summary>
        /// دریافت مقدار کیف پول
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<double>), 200)]
        public async Task<IActionResult> Get()
        {
            // نقد + اعتبار کلاب قابل خرج در همه‌جا (جزئیات در CurrentUser: WalletCashAmount/WalletCreditAmount)
            var balance = await WalletService.GetDisplayBalanceAsync(_currentUserHelper.CurrentUser.UserId);
            return Ok(new Application.Common.Dto.Result.BaseResultDto<double>(true, balance.Total));
        }
    }
}
