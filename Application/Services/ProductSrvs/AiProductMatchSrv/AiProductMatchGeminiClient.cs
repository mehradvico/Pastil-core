using Application.Services.PastilAISrv.Provider;
using Application.Services.ProductSrvs.AiProductMatchSrv.Iface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.AiProductMatchSrv
{
    // فراخوانی مستقل و کاملاً جدا از PastilAiCompletionRouter — عمداً هیچ کدی از مسیر چت پاستیل‌AI
    // (که همین الان در Production فعال است) تغییر یا فراخوانی نمی‌کند تا صفر ریسک رگرسیون داشته باشد.
    // فقط برای خواندن تنظیمات/کلید همان Providerهای PastilAI (Gemini/AvalAI/GapGPT)، به‌صورت فقط-خواندنی
    // استفاده می‌شود. اگر Gemini پاسخ ندهد (مثلاً به‌خاطر محدودیت شبکه)، به‌ترتیب Order روی بقیه‌ی
    // Providerهای سازگار با OpenAI (AvalAI/GapGPT) که از قبل برای PastilAI تنظیم شده‌اند Fallback می‌زند.
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
            var provider = GetCandidateProviders(hasImages: true).FirstOrDefault();
            providerName = provider?.Name;
            return provider != null;
        }

        // اولویت با همان Provider تنظیم‌شده در AiProductMatchOptions.ProviderName (پیش‌فرض Gemini)؛
        // بعد از آن فقط Providerهای صراحتاً لیست‌شده در FallbackProviderNames (به همان ترتیب) —
        // نه کل فهرست PastilAI:Providers — تا یک زنجیره‌ی طولانی از Providerهای احتمالاً معیوب/
        // Rate-Limit‌شده‌ی چت باعث Timeout سمت کلاینت نشود.
        private List<PastilAiProviderDefinition> GetCandidateProviders(bool hasImages)
        {
            var orderedNames = new List<string> { _options.ProviderName }
                .Concat(_options.FallbackProviderNames ?? new List<string>())
                .ToList();

            return orderedNames
                .Select(name => _providerOptions.Providers.FirstOrDefault(p =>
                    string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
                .Where(p => p != null && p.Enabled && !string.IsNullOrWhiteSpace(p.ResolveApiKey()))
                .Where(p => !hasImages || p.SupportsImage)
                .Where(p => hasImages ? !string.IsNullOrWhiteSpace(p.VisionModel) : !string.IsNullOrWhiteSpace(p.TextModel))
                .ToList();
        }

        public async Task<AiProductMatchGeminiCallResult> GenerateJsonAsync(
            string systemInstruction,
            string userText,
            IReadOnlyList<(string MimeType, byte[] Bytes)> images,
            CancellationToken cancellationToken)
        {
            var hasImages = images != null && images.Count > 0;
            var candidates = GetCandidateProviders(hasImages);
            if (candidates.Count == 0)
                return AiProductMatchGeminiCallResult.Failure("provider_not_configured");

            AiProductMatchGeminiCallResult lastFailure = AiProductMatchGeminiCallResult.Failure("provider_not_configured");

            foreach (var provider in candidates)
            {
                var apiKey = provider.ResolveApiKey();

                var result = string.Equals(provider.Kind, "Gemini", StringComparison.OrdinalIgnoreCase)
                    ? await CallGeminiAsync(provider, apiKey, systemInstruction, userText, images, hasImages, cancellationToken)
                    : await CallOpenAiCompatibleAsync(provider, apiKey, systemInstruction, userText, images, hasImages, cancellationToken);

                if (result.IsSuccess)
                    return result;

                _logger.LogWarning(
                    "AiProductMatch provider {Provider} failed with {ErrorCode}; trying next provider if any.",
                    provider.Name, result.ErrorCode);

                lastFailure = result;
            }

            return lastFailure;
        }

        private async Task<AiProductMatchGeminiCallResult> CallGeminiAsync(
            PastilAiProviderDefinition provider,
            string apiKey,
            string systemInstruction,
            string userText,
            IReadOnlyList<(string MimeType, byte[] Bytes)> images,
            bool hasImages,
            CancellationToken cancellationToken)
        {
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

        // برای Providerهای سازگار با OpenAI (AvalAI/GapGPT) — همان شکل درخواست/پاسخ استفاده‌شده در
        // PastilAiCompletionRouter.CallOpenAiCompatibleAsync، فقط بدون پارس پاسخ به شکل چت (answer/scope)؛
        // اینجا فقط متن خام JSON پاسخ لازم است تا AiProductMatchGeminiResponseParser خودش پارسش کند.
        private async Task<AiProductMatchGeminiCallResult> CallOpenAiCompatibleAsync(
            PastilAiProviderDefinition provider,
            string apiKey,
            string systemInstruction,
            string userText,
            IReadOnlyList<(string MimeType, byte[] Bytes)> images,
            bool hasImages,
            CancellationToken cancellationToken)
        {
            var model = hasImages ? provider.VisionModel : provider.TextModel;
            if (string.IsNullOrWhiteSpace(model))
                return AiProductMatchGeminiCallResult.Failure("model_not_configured");

            var instructionRole = string.Equals(provider.InstructionRole, "developer", StringComparison.OrdinalIgnoreCase)
                ? "developer"
                : "system";

            var userContent = new JsonArray
            {
                new JsonObject { ["type"] = "text", ["text"] = userText }
            };

            if (hasImages)
            {
                foreach (var image in images)
                {
                    userContent.Add(new JsonObject
                    {
                        ["type"] = "image_url",
                        ["image_url"] = new JsonObject
                        {
                            ["url"] = $"data:{image.MimeType};base64,{Convert.ToBase64String(image.Bytes)}"
                        }
                    });
                }
            }

            var messages = new JsonArray
            {
                new JsonObject { ["role"] = instructionRole, ["content"] = systemInstruction },
                new JsonObject { ["role"] = "user", ["content"] = hasImages ? userContent : (JsonNode)userText }
            };

            var payload = new JsonObject
            {
                ["model"] = model,
                ["messages"] = messages
            };

            if (provider.UseTemperature)
                payload["temperature"] = 0.4;

            if (provider.UseJsonResponseFormat)
                payload["response_format"] = new JsonObject { ["type"] = "json_object" };

            var path = string.IsNullOrWhiteSpace(provider.ChatCompletionsPath)
                ? "chat/completions"
                : provider.ChatCompletionsPath.Trim('/');
            var url = $"{provider.BaseUrl.TrimEnd('/')}/{path}";

            var http = _httpClientFactory.CreateClient();

            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(_options.RequestTimeoutSeconds, 5, 180)));

                using var message = new HttpRequestMessage(HttpMethod.Post, url);
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                message.Content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json");

                using var response = await http.SendAsync(message, timeoutCts.Token);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AiProductMatch {Provider} call returned {StatusCode}: {Body}", provider.Name, (int)response.StatusCode, body);
                    return AiProductMatchGeminiCallResult.Failure($"http_{(int)response.StatusCode}");
                }

                var root = JsonNode.Parse(body);
                var text = ExtractOpenAiContent(root?["choices"]?[0]?["message"]?["content"]);
                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning("AiProductMatch {Provider} call returned no usable text. Raw body: {Body}", provider.Name, body);
                    return AiProductMatchGeminiCallResult.Failure("empty_response");
                }

                return AiProductMatchGeminiCallResult.Success(text);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("AiProductMatch {Provider} call timed out after {Timeout}s.", provider.Name, _options.RequestTimeoutSeconds);
                return AiProductMatchGeminiCallResult.Failure("timeout");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AiProductMatch {Provider} call threw {ExceptionType}.", provider.Name, ex.GetType().Name);
                return AiProductMatchGeminiCallResult.Failure("exception:" + ex.GetType().Name);
            }
        }

        private static string ExtractOpenAiContent(JsonNode contentNode)
        {
            if (contentNode == null)
                return null;

            if (contentNode is JsonValue value && value.TryGetValue<string>(out var text))
                return text;

            if (contentNode is not JsonArray parts)
                return contentNode.ToJsonString();

            var builder = new StringBuilder();
            foreach (var part in parts)
            {
                var partText = part?["text"]?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(partText))
                    builder.Append(partText);
            }

            return builder.ToString();
        }
    }
}
