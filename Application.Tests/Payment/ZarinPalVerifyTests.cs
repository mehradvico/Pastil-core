using Application.Services.Order.PaymentGatewaySrv.Gateways;
using Entities.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Text;
using Xunit;

namespace Application.Tests.Payment;

public class ZarinPalVerifyTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;
        public int Calls { get; private set; }

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => _respond = respond;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(_respond(request));
        }
    }

    private static HttpResponseMessage Json(string body, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    private static async Task<(Application.Services.Order.PaymentGatewaySrv.Dto.GatewayCallbackResultDto Result, int Calls)> Verify(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = new StubHandler(respond);
        var gateway = new ZarinPalGateway(new HttpClient(handler), null, NullLogger<ZarinPalGateway>.Instance);
        var result = await gateway.VerifyStoredAuthorityAsync(new Entities.Entities.Payment { Id = 1, Token = "A000000000000000000000000000abc", Amount = 182000 }, new Merchant { MerchantNo = "m" });
        return (result, handler.Calls);
    }

    [Fact]
    public async Task Code100_IsSuccess()
    {
        var (result, calls) = await Verify(_ => Json("{\"data\":{\"code\":100,\"ref_id\":93620525201},\"errors\":[]}"));
        Assert.True(result.IsSuccess);
        Assert.False(result.IsPending);
        Assert.Equal("93620525201", result.RefNumber);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Code101_AlreadyVerified_IsSuccess()
    {
        var (result, _) = await Verify(_ => Json("{\"data\":{\"code\":101,\"ref_id\":93620525201},\"errors\":[]}"));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DefinitiveNegativeAnswer_IsFailure_NotPending()
    {
        var (result, calls) = await Verify(_ => Json("{\"data\":[],\"errors\":{\"code\":-53,\"message\":\"x\"}}"));
        Assert.False(result.IsSuccess);
        Assert.False(result.IsPending);
        Assert.Equal("-53", result.Description);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task NetworkError_IsPending_AfterRetry_NotFailed()
    {
        var (result, calls) = await Verify(_ => throw new HttpRequestException("boom"));
        Assert.False(result.IsSuccess);
        Assert.True(result.IsPending);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task ServerError_IsPending()
    {
        var (result, _) = await Verify(_ => Json("{\"message\":\"bad gateway\"}", HttpStatusCode.BadGateway));
        Assert.True(result.IsPending);
    }

    [Fact]
    public async Task GarbageBody_IsPending()
    {
        var (result, _) = await Verify(_ => Json("<html>oops</html>"));
        Assert.True(result.IsPending);
    }

    [Fact]
    public async Task FirstAttemptFails_SecondSucceeds()
    {
        var attempt = 0;
        var (result, calls) = await Verify(_ => ++attempt == 1
            ? throw new TaskCanceledException()
            : Json("{\"data\":{\"code\":101,\"ref_id\":5},\"errors\":[]}"));
        Assert.True(result.IsSuccess);
        Assert.Equal(2, calls);
    }
}
