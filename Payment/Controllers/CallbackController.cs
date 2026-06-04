using Application.Common.Helpers;
using Application.Common.Helpers.Iface;
using Application.Services.Order.PaymentSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Payment.Controllers
{
    public class CallbackController : Controller
    {
        private readonly ILogger<CallbackController> _logger;
        private readonly IPaymentService _paymentService;
        private readonly IAdminSettingHelper _adminSettingHelper;
        public CallbackController(ILogger<CallbackController> logger, IPaymentService paymentService, IAdminSettingHelper adminSettingHelper)
        {
            _logger = logger;
            _paymentService = paymentService;
            _adminSettingHelper = adminSettingHelper;
        }
        [Route("callback/{id}")]
        public async Task<IActionResult> Index(long id, bool test = false)
        {
            var payment = await _paymentService.CallbackPayment(id, test);

            TempData["ReturnToSiteUrl"] = AppSettingsHelper.BaseUrl;
            return View(payment);
        }
    }
}