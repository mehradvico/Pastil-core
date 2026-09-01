using Entities.Entities.PastilAIField;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilAISrv.Provider
{
    public class PastilAiCompletionRouter : IPastilAiCompletionRouter
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PastilAiProviderOptions _options;

        public PastilAiCompletionRouter(
            IHttpClientFactory httpClientFactory,
            IOptions<PastilAiProviderOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        public async Task<PastilAiRoutedResponse> CompleteAsync(
            PastilAiProviderRequest request,
            CancellationToken cancellationToken)
        {
            var routed = new PastilAiRoutedResponse();
            var preferredProvider = request.PreferredProvider?.Trim();

            var providers = _options.Providers
                .Select(provider => new
                {
                    Provider = provider,
                    ApiKey = provider.ResolveApiKey()
                })
                .Where(x => x.Provider.Enabled && !string.IsNullOrWhiteSpace(x.ApiKey))
                .Where(x => request.InputType != PastilAiInputType.Image || x.Provider.SupportsImage)
                .Where(x => request.InputType != PastilAiInputType.Audio || x.Provider.SupportsAudio)
                .Where(x => request.InputType != PastilAiInputType.Video || x.Provider.SupportsVideo)
                .Where(x => request.MediaDataUrl == null
                    ? !string.IsNullOrWhiteSpace(x.Provider.TextModel)
                    : !string.IsNullOrWhiteSpace(x.Provider.VisionModel))
                .OrderBy(x => string.IsNullOrWhiteSpace(preferredProvider) ||
                              !string.Equals(x.Provider.Name, preferredProvider, StringComparison.OrdinalIgnoreCase)
                    ? 1
                    : 0)
                .ThenBy(x => x.Provider.Order)
                .ToList();

            var attemptOrder = 0;

            foreach (var candidate in providers)
            {
                var provider = candidate.Provider;
                var currentAttemptOrder = ++attemptOrder;
                var started = DateTime.UtcNow;

                PastilAiProviderResponse response;

                try
                {
                    using var timeout =
                        CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                    timeout.CancelAfter(
                        TimeSpan.FromSeconds(
                            Math.Clamp(_options.RequestTimeoutSeconds, 5, 180)));

                    response = string.Equals(
                        provider.Kind,
                        "Gemini",
                        StringComparison.OrdinalIgnoreCase)
                        ? await CallGeminiAsync(
                            provider,
                            candidate.ApiKey,
                            request,
                            timeout.Token)
                        : await CallOpenAiCompatibleAsync(
                            provider,
                            candidate.ApiKey,
                            request,
                            timeout.Token);
                }
                catch (OperationCanceledException)
                    when (!cancellationToken.IsCancellationRequested)
                {
                    response = new PastilAiProviderResponse
                    {
                        IsSuccess = false,
                        ErrorCode = "timeout",
                        ErrorMessage = "Provider request timed out."
                    };
                }
                catch (Exception ex)
                {
                    response = new PastilAiProviderResponse
                    {
                        IsSuccess = false,
                        ErrorCode = "provider_exception",
                        ErrorMessage = ex.Message
                    };
                }

                var model = request.MediaDataUrl == null
                    ? provider.TextModel
                    : provider.VisionModel;

                response.Model ??= model;

                routed.Attempts.Add(new PastilAiProviderAttemptResult
                {
                    Provider = provider.Name,
                    Model = model,

                    Order = currentAttemptOrder,

                    StartDateUtc = started,
                    EndDateUtc = DateTime.UtcNow,
                    Response = response
                });

                if (!response.IsSuccess)
                    continue;

                routed.Provider = provider.Name;
                routed.Response = response;

                return routed;
            }
            routed.Response = routed.Attempts.LastOrDefault()?.Response ??
                new PastilAiProviderResponse
                {
                    IsSuccess = false,
                    ErrorCode = "no_provider",
                    ErrorMessage = string.IsNullOrWhiteSpace(preferredProvider)
                        ? "No enabled and configured provider can handle this request."
                        : $"No enabled and configured provider can handle this request. Preferred provider: {preferredProvider}."
                };

            return routed;
        }

        private async Task<PastilAiProviderResponse> CallOpenAiCompatibleAsync(
            PastilAiProviderDefinition provider,
            string apiKey,
            PastilAiProviderRequest request,
            CancellationToken cancellationToken)
        {
            var instructionRole = string.Equals(
                provider.InstructionRole,
                "developer",
                StringComparison.OrdinalIgnoreCase)
                ? "developer"
                : "system";

            var messages = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = instructionRole,
                    ["content"] = request.SystemPrompt
                }
            };

            foreach (var history in request.History)
            {
                messages.Add(new JsonObject
                {
                    ["role"] = history.Role == PastilAiMessageRole.Assistant ? "assistant" : "user",
                    ["content"] = history.Content
                });
            }

            if (request.MediaDataUrl == null)
            {
                messages.Add(new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = request.UserMessage
                });
            }
            else
            {
                messages.Add(new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["type"] = "text",
                            ["text"] = request.UserMessage
                        },
                        CreateOpenAiMediaPart(request)
                    }
                });
            }

            var payload = new JsonObject
            {
                ["model"] = request.MediaDataUrl == null ? provider.TextModel : provider.VisionModel,
                ["messages"] = messages
            };

            // 0.2 read as flat/robotic in practice; 0.55 keeps JSON-structured
            // answers reliable while giving them a more natural, less
            // canned voice.
            if (provider.UseTemperature)
                payload["temperature"] = 0.55;

            if (provider.UseJsonResponseFormat)
            {
                payload["response_format"] = new JsonObject
                {
                    ["type"] = "json_object"
                };
            }

            if (!string.IsNullOrWhiteSpace(provider.ThinkingMode))
            {
                payload["thinking"] = new JsonObject
                {
                    ["type"] = provider.ThinkingMode.Trim().ToLowerInvariant()
                };
            }

            var path = string.IsNullOrWhiteSpace(provider.ChatCompletionsPath)
                ? "chat/completions"
                : provider.ChatCompletionsPath.Trim('/');
            var url = $"{provider.BaseUrl.TrimEnd('/')}/{path}";

            var http = _httpClientFactory.CreateClient();
            using var message = new HttpRequestMessage(HttpMethod.Post, url);
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            message.Content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json");

            using var response = await http.SendAsync(message, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
                return Failure((int)response.StatusCode, body);

            var root = JsonNode.Parse(body);
            var content = ExtractOpenAiContent(root?["choices"]?[0]?["message"]?["content"]);
            var parsed = ParseModelOutput(content);
            parsed.HttpStatusCode = (int)response.StatusCode;
            parsed.PromptTokens = root?["usage"]?["prompt_tokens"]?.GetValue<int?>();
            parsed.CompletionTokens = root?["usage"]?["completion_tokens"]?.GetValue<int?>();
            return parsed;
        }

        private async Task<PastilAiProviderResponse> CallGeminiAsync(
            PastilAiProviderDefinition provider,
            string apiKey,
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
            {
                contents.Add(new JsonObject
                {
                    ["role"] = history.Role == PastilAiMessageRole.Assistant ? "model" : "user",
                    ["parts"] = new JsonArray
                    {
                        new JsonObject { ["text"] = history.Content }
                    }
                });
            }

            contents.Add(new JsonObject
            {
                ["role"] = "user",
                ["parts"] = parts
            });

            var payload = new JsonObject
            {
                ["system_instruction"] = new JsonObject
                {
                    ["parts"] = new JsonArray
                    {
                        new JsonObject { ["text"] = request.SystemPrompt }
                    }
                },
                ["contents"] = contents,
                ["generationConfig"] = new JsonObject
                {
                    ["temperature"] = 0.55,
                    ["responseMimeType"] = "application/json"
                }
            };

            var model = request.MediaDataUrl == null ? provider.TextModel : provider.VisionModel;
            var url = $"{provider.BaseUrl.TrimEnd('/')}/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";
            var http = _httpClientFactory.CreateClient();

            using var response = await http.PostAsync(
                url,
                new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json"),
                cancellationToken);

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

        private static PastilAiProviderResponse ParseModelOutput(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new PastilAiProviderResponse
                {
                    IsSuccess = false,
                    ErrorCode = "empty_response",
                    ErrorMessage = "Provider returned an empty response."
                };
            }

            try
            {
                var normalized = content.Trim();
                if (normalized.StartsWith("```", StringComparison.Ordinal))
                {
                    normalized = normalized
                        .Replace("```json", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("```", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Trim();
                }

                var node = JsonNode.Parse(normalized);
                var answer = node?["answer"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(answer))
                    throw new InvalidOperationException("answer is missing");

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
                return new PastilAiProviderResponse
                {
                    IsSuccess = false,
                    ErrorCode = "invalid_model_output",
                    ErrorMessage = ex.Message
                };
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
            {
                return new JsonObject
                {
                    ["type"] = "video_url",
                    ["video_url"] = new JsonObject
                    {
                        ["url"] = request.MediaDataUrl
                    }
                };
            }

            return new JsonObject
            {
                ["type"] = "image_url",
                ["image_url"] = new JsonObject
                {
                    ["url"] = request.MediaDataUrl
                }
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
