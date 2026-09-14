using Application.Services.PastilAISrv.Provider;
using Application.Services.ProductSrvs.AiProductMatchSrv.Iface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.AiProductMatchSrv
{
    // فراخوانی مستقل و کاملاً جدا از PastilAiCompletionRouter — عمداً هیچ کدی از مسیر چت پاستیل‌AI
    // (که همین الان در Production فعال است) تغییر یا فراخوانی نمی‌کند تا صفر ریسک رگرسیون داشته باشد.
    // فقط برای خواندن تنظیمات/کلید موجود Gemini، از همان PastilAiProviderOptions به‌صورت فقط-خواندنی
    // استفاده می‌شود (طبق تصمیم ۰.۱ سند طرح پیاده‌سازی).
    public class AiProductMatchGeminiClient : IAiProductMatchGeminiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PastilAiProviderOptions _providerOptions;
        private readonly AiProductMatchOptions _options;
        private readonly ILogger<AiProductMatchGeminiClient> _logger;

        public AiProductMatchGeminiClient(
            IHttpClientFactory httpClientFactory,
            IOptions<PastilAiProviderOptions> providerOptions,
            IOptions<AiProductMatchOptions> options,
            ILogger<AiProductMatchGeminiClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _providerOptions = providerOptions.Value;
            _options = options.Value;
            _logger = logger;
        }

        public bool IsAvailable(out string providerName)
        {
            var provider = ResolveProvider();
            providerName = provider?.Name;
            return provider != null && !string.IsNullOrWhiteSpace(provider.ResolveApiKey());
        }

        private PastilAiProviderDefinition ResolveProvider()
            => _providerOptions.Providers.FirstOrDefault(p =>
                p.Enabled &&
                string.Equals(p.Name, _options.ProviderName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.Kind, "Gemini", StringComparison.OrdinalIgnoreCase));

        public async Task<AiProductMatchGeminiCallResult> GenerateJsonAsync(
            string systemInstruction,
            string userText,
            IReadOnlyList<(string MimeType, byte[] Bytes)> images,
            CancellationToken cancellationToken)
        {
            var provider = ResolveProvider();
            if (provider == null)
                return AiProductMatchGeminiCallResult.Failure("provider_not_configured");

            var apiKey = provider.ResolveApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
                return AiProductMatchGeminiCallResult.Failure("api_key_missing");

            var hasImages = images != null && images.Count > 0;
            var model = hasImages ? provider.VisionModel : provider.TextModel;
            if (string.IsNullOrWhiteSpace(model))
                return AiProductMatchGeminiCallResult.Failure("model_not_configured");

            var parts = new JsonArray();
            if (hasImages)
            {
                foreach (var image in images)
                {
                    parts.Add(new JsonObject
                    {
                        ["inline_data"] = new JsonObject
                        {
                            ["mime_type"] = image.MimeType,
                            ["data"] = Convert.ToBase64String(image.Bytes)
                        }
                    });
                }
            }
            parts.Add(new JsonObject { ["text"] = userText });

            var payload = new JsonObject
            {
                ["system_instruction"] = new JsonObject
                {
                    ["parts"] = new JsonArray { new JsonObject { ["text"] = systemInstruction } }
                },
                ["contents"] = new JsonArray
                {
                    new JsonObject { ["role"] = "user", ["parts"] = parts }
                },
                ["generationConfig"] = new JsonObject
                {
                    ["temperature"] = 0.4,
                    ["responseMimeType"] = "application/json"
                }
            };

            var url = $"{provider.BaseUrl.TrimEnd('/')}/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";
            var http = _httpClientFactory.CreateClient();

            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(_options.RequestTimeoutSeconds, 5, 180)));

                using var response = await http.PostAsync(
                    url,
                    new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json"),
                    timeoutCts.Token);

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AiProductMatch Gemini call returned {StatusCode}: {Body}", (int)response.StatusCode, body);
                    return AiProductMatchGeminiCallResult.Failure($"http_{(int)response.StatusCode}");
                }

                var root = JsonNode.Parse(body);
                var text = root?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning("AiProductMatch Gemini call returned no usable text. Raw body: {Body}", body);
                    return AiProductMatchGeminiCallResult.Failure("empty_response");
                }

                return AiProductMatchGeminiCallResult.Success(text);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("AiProductMatch Gemini call timed out after {Timeout}s.", _options.RequestTimeoutSeconds);
                return AiProductMatchGeminiCallResult.Failure("timeout");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AiProductMatch Gemini call threw {ExceptionType}.", ex.GetType().Name);
                return AiProductMatchGeminiCallResult.Failure("exception:" + ex.GetType().Name);
            }
        }
    }
}
