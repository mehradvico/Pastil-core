using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.PaymentSrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت کیف پول
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService WalletService;
        private readonly ICurrentUserHelper _currentUserHelper;
        private readonly IPaymentService _paymentService;

        /// <summary>
        /// مدیریت کیف پول
        /// </summary>
        ///
        public WalletController(IWalletService WalletService, ICurrentUserHelper currentUserHelper, IPaymentService paymentService)
        {
            this.WalletService = WalletService;
            this._currentUserHelper = currentUserHelper;
            this._paymentService = paymentService;
        }
        /// <summary>
        /// تاریخچه کیف پول 
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseInputDto), 200)]
        public IActionResult Get([FromQuery] WalletInputDto dto)
        {
            dto.UserId = _currentUserHelper.CurrentUser.UserId;
            var searchDto = WalletService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PaymentStartDto>), 200)]
        public async Task<IActionResult> Post(PaymentStartDto payment)
        {
            payment.UserId = _currentUserHelper.CurrentUser.UserId;
            payment.User = new Application.Services.Dto.UserMinVDto() { Email = _currentUserHelper.CurrentUser.Email, Mobile = _currentUserHelper.CurrentUser.Mobile, Id = _currentUserHelper.CurrentUser.UserId };
            var dto = await _paymentService.InsertWalletPaymentAsyncDto(payment);
            return Ok(dto);
        }

        /// <summary>
        /// حداقل و حداکثر مبلغ شارژ کیف پول (تومان) برای اعتبارسنجی فرم
        /// </summary>
        [HttpGet("ChargeLimits")]
        [ProducesResponseType(typeof(BaseResultDto<WalletChargeLimitsDto>), 200)]
        public IActionResult ChargeLimits()
        {
            return Ok(new BaseResultDto<WalletChargeLimitsDto>(true, new WalletChargeLimitsDto
            {
                MinAmount = Application.Services.ProductSrvs.WalletSrv.WalletChargeRules.MinAmount,
                MaxAmount = Application.Services.ProductSrvs.WalletSrv.WalletChargeRules.MaxAmount
            }));
        }
    }
}
