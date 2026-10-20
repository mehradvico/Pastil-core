using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.Order.PaymentSrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

        // توکن از مسیر (آدرس‌های تازه: callback/{id}/{token}) یا query (پرداخت‌های قبل از انتشار: ?callbackToken=).
        // بعضی بانک‌ها (پارسیان) نتیجه را با فرم POST برمی‌گردانند؛ امنیت این مسیر توکن تصادفی است، نه antiforgery.
        [AcceptVerbs("GET", "POST")]
        [IgnoreAntiforgeryToken]
        [EnableRateLimiting("PaymentCallback")]
        [Route("callback/{id:long}")]
        [Route("callback/{id:long}/{callbackToken}")]
        public async Task<IActionResult> Index(long id, string callbackToken)
        {
            try
            {
                var payment = await _paymentService.CallbackPayment(id, callbackToken);

                if (!payment.IsSuccess)
                {
                    _logger.LogWarning(
                        "Payment callback failed. PaymentId: {PaymentId}, Messages: {Messages}",
                        id,
                        string.Join(" | ", payment.Messages.Select(x => x.Item1)));
                }

                TempData["ReturnToSiteUrl"] = AppSettingsHelper.BaseUrl;
                ViewData["ReturnLinks"] = Models.PaymentReturnLinks.For(payment.Data, payment.IsSuccess, AppSettingsHelper.BaseUrl);
                return View("Index", payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled payment callback error. PaymentId: {PaymentId}", id);

                TempData["ReturnToSiteUrl"] = AppSettingsHelper.BaseUrl;
                ViewData["ReturnLinks"] = Models.PaymentReturnLinks.For(null, false, AppSettingsHelper.BaseUrl);

                var failedResult = new BaseResultDto<PaymentDto>(
                    false,
                    "خطایی هنگام پردازش نتیجه پرداخت رخ داد.",
                    null!);

                return View("Index", failedResult);
            }
        }
    }
}
