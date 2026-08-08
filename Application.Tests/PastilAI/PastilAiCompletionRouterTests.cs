using Application.Services.PastilAISrv.Provider;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using Xunit;

namespace Application.Tests.PastilAI;

public class PastilAiCompletionRouterTests
{
    [Fact]
    public async Task Falls_back_to_next_provider_after_http_failure()
    {
        var handler = new StubHandler(request =>
        {
            if (request.RequestUri!.Host == "first.test")
                return Json(HttpStatusCode.TooManyRequests, """{"error":"rate limited"}""");
            return Json(HttpStatusCode.OK,
                """{"choices":[{"message":{"content":"{\"answer\":\"پاسخ\",\"scope\":\"PetGeneral\",\"isEmergency\":false}"}}],"usage":{"prompt_tokens":5,"completion_tokens":2}}""");
        });
        var router = CreateRouter(handler, Provider("First", "https://first.test/v1", 1), Provider("Second", "https://second.test/v1", 2));

        var result = await router.CompleteAsync(new PastilAiProviderRequest
        {
            SystemPrompt = "system",
            UserMessage = "چرا گربه من زیاد می‌خوابد؟"
        }, CancellationToken.None);

        Assert.True(result.Response.IsSuccess);
        Assert.Equal("Second", result.Provider);
        Assert.Equal(2, result.Attempts.Count);
        Assert.False(result.Attempts[0].Response.IsSuccess);
        Assert.Equal("پاسخ", result.Response.Answer);
    }

    [Fact]
    public async Task Invalid_json_triggers_fallback()
    {
        var call = 0;
        var handler = new StubHandler(_ =>
        {
            call++;
            var content = call == 1 ? "not-json" : """{"answer":"درست","scope":"PastilData","isEmergency":false}""";
            return Json(HttpStatusCode.OK, System.Text.Json.JsonSerializer.Serialize(new
            {
                choices = new[] { new { message = new { content } } }
            }));
        });
        var router = CreateRouter(handler, Provider("First", "https://same.test/v1", 1), Provider("Second", "https://same.test/v1", 2));

        var result = await router.CompleteAsync(new PastilAiProviderRequest
        {
            SystemPrompt = "system",
            UserMessage = "محصول"
        }, CancellationToken.None);

        Assert.True(result.Response.IsSuccess);
        Assert.Equal("Second", result.Provider);
        Assert.Equal("invalid_model_output", result.Attempts[0].Response.ErrorCode);
    }

    [Fact]
    public async Task Image_request_skips_provider_without_vision()
    {
        var handler = new StubHandler(_ => Json(HttpStatusCode.OK,
            """{"choices":[{"message":{"content":"{\"answer\":\"تصویر\",\"scope\":\"PetGeneral\",\"isEmergency\":false}"}}]}"""));
        var textOnly = Provider("TextOnly", "https://text.test/v1", 1);
        textOnly.SupportsImage = false;
        var vision = Provider("Vision", "https://vision.test/v1", 2);
        var router = CreateRouter(handler, textOnly, vision);

        var result = await router.CompleteAsync(new PastilAiProviderRequest
        {
            SystemPrompt = "system",
            UserMessage = "این عکس چیست؟",
            MediaDataUrl = "data:image/png;base64,AA==",
            InputType = Entities.Entities.PastilAIField.PastilAiInputType.Image
        }, CancellationToken.None);

        Assert.True(result.Response.IsSuccess);
        Assert.Equal("Vision", result.Provider);
        Assert.Single(result.Attempts);
    }

    private static PastilAiCompletionRouter CreateRouter(HttpMessageHandler handler, params PastilAiProviderDefinition[] providers)
    {
        var options = Options.Create(new PastilAiProviderOptions
        {
            RequestTimeoutSeconds = 5,
            Providers = providers.ToList()
        });
        return new PastilAiCompletionRouter(new StubHttpClientFactory(handler), options);
    }

    private static PastilAiProviderDefinition Provider(string name, string baseUrl, int order) => new()
    {
        Name = name,
        Kind = "OpenAI",
        BaseUrl = baseUrl,
        ApiKey = "test-key",
        TextModel = "test-text",
        VisionModel = "test-vision",
        Order = order,
        Enabled = true,
        SupportsImage = true,
        SupportsAudio = true,
        SupportsVideo = true
    };

    private static HttpResponseMessage Json(HttpStatusCode status, string json) => new(status)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(responder(request));
    }

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }
}
