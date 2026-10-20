using Application.Services.Order.ShippingSrv;
using Application.Services.Order.ShippingSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
    /// <summary>
    /// دریافت رویدادهای Webhook میاره (added, state_changed, courier_assigned, arrived,
    /// picked_up, departed, delivered, batched). میاره تا ۵ بار با فاصله‌ی ۵ ثانیه روی پاسخ
    /// غیر ۲XX دوباره تلاش می‌کند، پس این Endpoint همیشه سریع ۲۰۰ برمی‌گرداند و خطاها را فقط لاگ می‌کند.
    /// </summary>
    [Route("api/webhooks/miare")]
    [ApiController]
    [AllowAnonymous]
    public class MiareWebhookController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;
        private readonly ShippingOptions _options;
        private readonly ILogger<MiareWebhookController> _logger;

        public MiareWebhookController(
            IShipmentService shipmentService,
            IOptions<ShippingOptions> options,
            ILogger<MiareWebhookController> logger)
        {
            _shipmentService = shipmentService;
            _options = options.Value;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Receive(CancellationToken cancellationToken)
        {
            var expectedKey = _options.Miare.ApiKey;
            if (string.IsNullOrWhiteSpace(expectedKey))
            {
                _logger.LogWarning("Miare webhook rejected: Shipping:Miare:ApiKey is not configured on this server.");
                return Unauthorized();
            }

            var authHeader = Request.Headers["Authorization"].ToString();
            // مقایسه‌ی زمان‌ثابت تا با اندازه‌گیری زمان پاسخ نتوان کلید را حدس زد
            if (!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(authHeader),
                    System.Text.Encoding.UTF8.GetBytes($"Token {expectedKey}")))
            {
                // بدون این لاگ، وب‌هوکی که میاره با هدر/کلید اشتباه می‌فرستد هیچ ردی نداشت و «میاره زنگ نمی‌زند» با «کلید اشتباه» یکی دیده می‌شد
                _logger.LogWarning("Miare webhook rejected: the Authorization header does not match (header present: {HasHeader}).",
                    !string.IsNullOrEmpty(authHeader));
                return Unauthorized();
            }

            string payload;
            using (var reader = new StreamReader(Request.Body))
                payload = await reader.ReadToEndAsync();

            try
            {
                await _shipmentService.HandleMiareWebhookAsync(payload, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to process Miare webhook payload.");
            }

            return Ok();
        }
    }
}
