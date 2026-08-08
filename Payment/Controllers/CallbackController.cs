using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.Order.PaymentSrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Payment.Controllers
{
    public class CallbackController : Controller
    {
        private readonly ILogger<CallbackController> _logger;
        private readonly IPaymentService _paymentService;

        public CallbackController(
            ILogger<CallbackController> logger,
            IPaymentService paymentService)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        [HttpGet]
        [Route("callback/{id:long}")]
        public async Task<IActionResult> Index(long id)
        {
            try
            {
                var payment = await _paymentService.CallbackPayment(id);

                if (!payment.IsSuccess)
                {
                    _logger.LogWarning(
                        "Payment callback failed. PaymentId: {PaymentId}, Messages: {Messages}",
                        id,
                        string.Join(" | ", payment.Messages.Select(x => x.Item1)));
                }

                TempData["ReturnToSiteUrl"] = AppSettingsHelper.BaseUrl;
                return View("Index", payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled payment callback error. PaymentId: {PaymentId}", id);

                TempData["ReturnToSiteUrl"] = AppSettingsHelper.BaseUrl;

                var failedResult = new BaseResultDto<PaymentDto>(
                    false,
                    "خطایی هنگام پردازش نتیجه پرداخت رخ داد.",
                    null!);

                return View("Index", failedResult);
            }
        }
    }
}
