using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.PaymentSrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت پرداخت رزرو خدمات ها
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionReservePaymentController : Controller
    {
        private readonly IPansionReserveService _PansionReserveService;
        private readonly ICurrentUserHelper _currentUserHelper;
        private readonly IPaymentService _paymentService;
        public PansionReservePaymentController(IPansionReserveService PansionReserveService, ICurrentUserHelper currentUserHelper, IPaymentService paymentService)
        {
            this._PansionReserveService = PansionReserveService;
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
            var dto = await _paymentService.InsertPansionReservePaymentAsyncDto(payment);
            return Ok(dto);
        }
    }
}
