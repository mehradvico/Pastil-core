using Entities.Entities.PastilAIField;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilAISrv.Provider
{
    public class PastilAiCompletionRouter : IPastilAiCompletionRouter
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PastilAiProviderOptions _options;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public PastilAiCompletionRouter(IHttpClientFactory httpClientFactory, IOptions<PastilAiProviderOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        public async Task<PastilAiRoutedResponse> CompleteAsync(PastilAiProviderRequest request, CancellationToken cancellationToken)
        {
            var routed = new PastilAiRoutedResponse();
            var providers = _options.Providers
                .Where(x => x.Enabled && !string.IsNullOrWhiteSpace(x.ApiKey))
                .Where(x => request.InputType != PastilAiInputType.Image || x.SupportsImage)
                .Where(x => request.InputType != PastilAiInputType.Audio || x.SupportsAudio)
                .Where(x => request.InputType != PastilAiInputType.Video || x.SupportsVideo)
                .Where(x => request.MediaDataUrl == null
                    ? !string.IsNullOrWhiteSpace(x.TextModel)
                    : !string.IsNullOrWhiteSpace(x.VisionModel))
                .OrderBy(x => x.Order)
                .ToList();

            foreach (var provider in providers)
            {
                var started = DateTime.UtcNow;
                PastilAiProviderResponse response;
                try
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(_options.RequestTimeoutSeconds, 5, 180)));
                    response = string.Equals(provider.Kind, "Gemini", StringComparison.OrdinalIgnoreCase)
                        ? await CallGeminiAsync(provider, request, timeout.Token)
                        : await CallOpenAiCompatibleAsync(provider, request, timeout.Token);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    response = new PastilAiProviderResponse { IsSuccess = false, ErrorCode = "timeout", ErrorMessage = "Provider request timed out." };
                }
                catch (Exception ex)
                {
                    response = new PastilAiProviderResponse { IsSuccess = false, ErrorCode = "provider_exception", ErrorMessage = ex.Message };
                }

                var model = request.MediaDataUrl == null ? provider.TextModel : provider.VisionModel;
                response.Model ??= model;
                routed.Attempts.Add(new PastilAiProviderAttemptResult
                {
                    Provider = provider.Name,
                    Model = model,
                    Order = provider.Order,
                    StartDateUtc = started,
                    EndDateUtc = DateTime.UtcNow,
                    Response = response
                });

                if (response.IsSuccess)
                {
                    routed.Provider = provider.Name;
                    routed.Response = response;
                    return routed;
                }
            }

            routed.Response = routed.Attempts.LastOrDefault()?.Response ??
                new PastilAiProviderResponse { IsSuccess = false, ErrorCode = "no_provider", ErrorMessage = "No enabled provider can handle this request." };
            return routed;
        }

        private async Task<PastilAiProviderResponse> CallOpenAiCompatibleAsync(
            PastilAiProviderDefinition provider,
            PastilAiProviderRequest request,
            CancellationToken cancellationToken)
        {
            var messages = new JsonArray
            {
                new JsonObject { ["role"] = "system", ["content"] = request.SystemPrompt }
            };
            foreach (var history in request.History)
                messages.Add(new JsonObject
                {
                    ["role"] = history.Role == PastilAiMessageRole.Assistant ? "assistant" : "user",
                    ["content"] = history.Content
                });

            if (request.MediaDataUrl == null)
            {
                messages.Add(new JsonObject { ["role"] = "user", ["content"] = request.UserMessage });
            }
            else
            {
                messages.Add(new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = new JsonArray
                    {
                        new JsonObject { ["type"] = "text", ["text"] = request.UserMessage },
                        CreateOpenAiMediaPart(request)
                    }
                });
            }

            var payload = new JsonObject
            {
                ["model"] = request.MediaDataUrl == null ? provider.TextModel : provider.VisionModel,
                ["messages"] = messages,
                ["temperature"] = 0.2,
                ["response_format"] = new JsonObject { ["type"] = "json_object" }
            };

            var http = _httpClientFactory.CreateClient();
            using var message = new HttpRequestMessage(HttpMethod.Post, $"{provider.BaseUrl.TrimEnd('/')}/chat/completions");
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", provider.ApiKey);
            message.Content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json");
            using var response = await http.SendAsync(message, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                return Failure((int)response.StatusCode, body);

            var root = JsonNode.Parse(body);
            var content = root?["choices"]?[0]?["message"]?["content"]?.GetValue<string>();
            var parsed = ParseModelOutput(content);
            parsed.HttpStatusCode = (int)response.StatusCode;
            parsed.PromptTokens = root?["usage"]?["prompt_tokens"]?.GetValue<int?>();
            parsed.CompletionTokens = root?["usage"]?["completion_tokens"]?.GetValue<int?>();
            return parsed;
        }

        private async Task<PastilAiProviderResponse> CallGeminiAsync(
            PastilAiProviderDefinition provider,
            PastilAiProviderRequest request,
            CancellationToken cancellationToken)
        {
            var parts = new JsonArray();
            if (request.MediaDataUrl != null)
            {
                var comma = request.MediaDataUrl.IndexOf(',');
                var meta = request.MediaDataUrl[..comma];
                var mime = meta[5..meta.IndexOf(';')];
                parts.Add(new JsonObject
                {
                    ["inline_data"] = new JsonObject
                    {
                        ["mime_type"] = mime,
                        ["data"] = request.MediaDataUrl[(comma + 1)..]
                    }
                });
            }
            parts.Add(new JsonObject { ["text"] = request.UserMessage });

            var contents = new JsonArray();
            foreach (var history in request.History)
                contents.Add(new JsonObject
                {
                    ["role"] = history.Role == PastilAiMessageRole.Assistant ? "model" : "user",
                    ["parts"] = new JsonArray { new JsonObject { ["text"] = history.Content } }
                });
            contents.Add(new JsonObject { ["role"] = "user", ["parts"] = parts });

            var payload = new JsonObject
            {
                ["system_instruction"] = new JsonObject
                {
                    ["parts"] = new JsonArray { new JsonObject { ["text"] = request.SystemPrompt } }
                },
                ["contents"] = contents,
                ["generationConfig"] = new JsonObject
                {
                    ["temperature"] = 0.2,
                    ["responseMimeType"] = "application/json"
                }
            };

            var model = request.MediaDataUrl == null ? provider.TextModel : provider.VisionModel;
            var url = $"{provider.BaseUrl.TrimEnd('/')}/models/{model}:generateContent?key={Uri.EscapeDataString(provider.ApiKey)}";
            var http = _httpClientFactory.CreateClient();
            using var response = await http.PostAsync(url, new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json"), cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                return Failure((int)response.StatusCode, body);

            var root = JsonNode.Parse(body);
            var content = root?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.GetValue<string>();
            var parsed = ParseModelOutput(content);
            parsed.HttpStatusCode = (int)response.StatusCode;
            parsed.PromptTokens = root?["usageMetadata"]?["promptTokenCount"]?.GetValue<int?>();
            parsed.CompletionTokens = root?["usageMetadata"]?["candidatesTokenCount"]?.GetValue<int?>();
            return parsed;
        }

        private static PastilAiProviderResponse ParseModelOutput(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new PastilAiProviderResponse { IsSuccess = false, ErrorCode = "empty_response", ErrorMessage = "Provider returned an empty response." };
            try
            {
                var normalized = content.Trim();
                if (normalized.StartsWith("```"))
                    normalized = normalized.Replace("```json", "", StringComparison.OrdinalIgnoreCase).Replace("```", "").Trim();
                var node = JsonNode.Parse(normalized);
                var answer = node?["answer"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(answer))
                    throw new JsonException("answer is missing");
                Enum.TryParse(node?["scope"]?.GetValue<string>(), true, out PastilAiScope scope);
                return new PastilAiProviderResponse
                {
                    IsSuccess = true,
                    Answer = answer.Trim(),
                    Scope = scope,
                    IsEmergency = node?["isEmergency"]?.GetValue<bool?>() ?? false
                };
            }
            catch (Exception ex)
            {
                return new PastilAiProviderResponse { IsSuccess = false, ErrorCode = "invalid_model_output", ErrorMessage = ex.Message };
            }
        }

        private static JsonObject CreateOpenAiMediaPart(PastilAiProviderRequest request)
        {
            if (request.InputType == PastilAiInputType.Audio)
            {
                var comma = request.MediaDataUrl.IndexOf(',');
                var mime = request.MediaDataUrl[5..request.MediaDataUrl.IndexOf(';')];
                var format = mime.Split('/').Last().Replace("mpeg", "mp3");
                return new JsonObject
                {
                    ["type"] = "input_audio",
                    ["input_audio"] = new JsonObject
                    {
                        ["data"] = request.MediaDataUrl[(comma + 1)..],
                        ["format"] = format
                    }
                };
            }
            if (request.InputType == PastilAiInputType.Video)
                return new JsonObject
                {
                    ["type"] = "video_url",
                    ["video_url"] = new JsonObject { ["url"] = request.MediaDataUrl }
                };
            return new JsonObject
            {
                ["type"] = "image_url",
                ["image_url"] = new JsonObject { ["url"] = request.MediaDataUrl }
            };
        }

        private static PastilAiProviderResponse Failure(int status, string body) => new()
        {
            IsSuccess = false,
            HttpStatusCode = status,
            ErrorCode = $"http_{status}",
            ErrorMessage = body?.Length > 2000 ? body[..2000] : body
        };
    }
}
