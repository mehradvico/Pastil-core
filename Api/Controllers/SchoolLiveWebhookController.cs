using Application.Services.SchoolSrvs.SchoolCourseLiveSrv;
using Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Iface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Controllers
{
    /// <summary>
    /// وب‌هوک LiveKit Egress - وقتی ضبط یک پخش زنده‌ی مدرسه تمام می‌شود (موفق یا ناموفق) اینجا خبر می‌رسد.
    /// این آدرس باید در تنظیمات پروژه‌ی LiveKit Cloud (Webhooks) با همین مسیر ثبت شود؛ بدون [Authorize]
    /// چون کاربرِ لاگین‌شده در کار نیست - امنیتش با اعتبارسنجی JWT هدر Authorization (طبق مستندات
    /// LiveKit) تأمین می‌شود، نه احراز هویت معمول اپلیکیشن.
    /// </summary>
    [Route("api/webhook/livekit-egress")]
    [ApiController]
    public class SchoolLiveWebhookController : ControllerBase
    {
        private readonly ISchoolCourseLiveService _liveService;
        private readonly LiveKitOptions _options;
        private readonly ILogger<SchoolLiveWebhookController> _logger;

        public SchoolLiveWebhookController(ISchoolCourseLiveService liveService, IOptions<LiveKitOptions> options, ILogger<SchoolLiveWebhookController> logger)
        {
            _liveService = liveService;
            _options = options.Value;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var rawBody = await reader.ReadToEndAsync();

            if (!VerifySignature(rawBody))
            {
                _logger.LogWarning("LiveKit Egress webhook rejected: invalid signature.");
                return Unauthorized();
            }

            try
            {
                using var json = JsonDocument.Parse(rawBody);
                var root = json.RootElement;
                var eventName = root.TryGetProperty("event", out var eventProp) ? eventProp.GetString() : null;
                if (eventName != "egress_ended")
                    return Ok();

                if (!root.TryGetProperty("egressInfo", out var egressInfo))
                    return Ok();

                var egressId = egressInfo.TryGetProperty("egressId", out var idProp) ? idProp.GetString() : null;
                var status = egressInfo.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;
                var succeeded = string.Equals(status, "EGRESS_COMPLETE", StringComparison.OrdinalIgnoreCase);

                string fileUrl = null;
                if (succeeded && egressInfo.TryGetProperty("fileResults", out var fileResults) && fileResults.GetArrayLength() > 0)
                {
                    var first = fileResults[0];
                    var filename = first.TryGetProperty("filename", out var f) ? f.GetString() : null;
                    fileUrl = !string.IsNullOrWhiteSpace(_options.RecordingPublicBaseUrl) && !string.IsNullOrWhiteSpace(filename)
                        ? $"{_options.RecordingPublicBaseUrl.TrimEnd('/')}/{filename.TrimStart('/')}"
                        : (first.TryGetProperty("location", out var loc) ? loc.GetString() : null);
                }

                if (!string.IsNullOrWhiteSpace(egressId))
                    await _liveService.HandleEgressWebhookAsync(egressId, succeeded, fileUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process LiveKit Egress webhook payload.");
            }

            return Ok();
        }

        private bool VerifySignature(string rawBody)
        {
            if (string.IsNullOrWhiteSpace(_options.WebhookApiSecret))
                return true; // برای محیط توسعه‌ای که هنوز کلید وب‌هوک تنظیم نشده - تیم عملیات باید قبل از Production این را الزامی کند

            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authHeader))
                return false;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.WebhookApiSecret));
                handler.ValidateToken(authHeader, new TokenValidationParameters
                {
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                }, out _);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
