using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.PaymentSrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت پرداخت سبد رزرو نمایندگان (پرداخت واحد برای چند رزرو یک نمایندگی)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveBatchPaymentController : Controller
    {
        private readonly ICurrentUserHelper _currentUserHelper;
        private readonly IPaymentService _paymentService;
        public CompanionReserveBatchPaymentController(ICurrentUserHelper currentUserHelper, IPaymentService paymentService)
        {
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
            var dto = await _paymentService.InsertCompanionReserveBatchPaymentAsyncDto(payment);
            return Ok(dto);
        }
    }
}
