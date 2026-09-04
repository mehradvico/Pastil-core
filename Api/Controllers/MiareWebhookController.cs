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
                return Unauthorized();

            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.Equals(authHeader, $"Token {expectedKey}", StringComparison.Ordinal))
                return Unauthorized();

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
