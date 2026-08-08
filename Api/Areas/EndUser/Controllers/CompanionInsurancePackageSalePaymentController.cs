using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionInsurancePackageSaleSrv.Iface;
using Application.Services.Content.CargoSrv.Iface;
using Application.Services.Order.PaymentSrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت پرداخت بیمه ها
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionInsurancePackageSalePaymentController : Controller
    {
        private readonly ICompanionInsurancePackageSaleService _insuranceService;
        private readonly ICurrentUserHelper _currentUserHelper;
        private readonly IPaymentService _paymentService;
        public CompanionInsurancePackageSalePaymentController(ICompanionInsurancePackageSaleService insuranceService, ICurrentUserHelper currentUserHelper, IPaymentService paymentService)
        {
            this._insuranceService = insuranceService;
            this._currentUserHelper = currentUserHelper;
            this._paymentService = paymentService;
        }

        /// <summary>
        /// آیتم جدید پرداختی
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PaymentStartDto>), 200)]
        public async Task<IActionResult> Post(PaymentStartDto payment)
        {
            payment.UserId = _currentUserHelper.CurrentUser.UserId;
            payment.User = new Application.Services.Dto.UserMinVDto() { Email = _currentUserHelper.CurrentUser.Email, Mobile = _currentUserHelper.CurrentUser.Mobile, Id = _currentUserHelper.CurrentUser.UserId };
            var dto = await _paymentService.InsertCompanionInsurancePackageSalePaymentAsyncDto(payment);
            return Ok(dto);
        }
    }
}
