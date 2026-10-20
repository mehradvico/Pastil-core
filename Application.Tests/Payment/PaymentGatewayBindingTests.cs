using Application.Services.Order.PaymentGatewaySrv;
using Application.Services.Order.PaymentGatewaySrv.Gateways;
using Application.Services.Order.PaymentSrv.Dto;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Application.Tests.Payment;

// نتیجه‌ی برگشتی بانک فقط برای همان پرداختی پذیرفته می‌شود که برایش شروع شده بود (و قبل از هر تماس شبکه‌ای رد می‌شود)
public class PaymentGatewayBindingTests
{
    private sealed class NoNetworkHandler : HttpMessageHandler
    {
        public bool Called { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Called = true;
            throw new InvalidOperationException("no network in tests");
        }
    }

    private static HttpRequest Request(string query)
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString(query);
        return context.Request;
    }

    private static Entities.Entities.Payment Payment(long id, string token, double amount = 50000) =>
        new() { Id = id, Token = token, Amount = amount, MerchantId = 1 };

    [Fact]
    public async Task ZarinPal_RejectsAuthorityOfAnotherPayment_WithoutCallingZarinPal()
    {
        var handler = new NoNetworkHandler();
        var gateway = new ZarinPalGateway(new HttpClient(handler), null, NullLogger<ZarinPalGateway>.Instance);

        var result = await gateway.CallbackAsync(Payment(5, "A000000000000000000000000000OWN"), new Merchant(), Request("?Status=OK&Authority=A000000000000000000000000000OTHER"));

        Assert.False(result.IsSuccess);
        Assert.Equal(Resource.Notification.InvalidData, result.ErrorMessage);
        Assert.False(handler.Called);
    }

    [Fact]
    public async Task ZarinPal_RejectsWhenNoAuthorityWasStored()
    {
        var handler = new NoNetworkHandler();
        var gateway = new ZarinPalGateway(new HttpClient(handler), null, NullLogger<ZarinPalGateway>.Instance);

        var result = await gateway.CallbackAsync(Payment(5, null), new Merchant(), Request("?Status=OK&Authority=A1"));

        Assert.False(result.IsSuccess);
        Assert.False(handler.Called);
    }

    [Fact]
    public async Task ZarinPal_StartFails_WhenCallbackUrlIsMissing()
    {
        var handler = new NoNetworkHandler();
        var gateway = new ZarinPalGateway(new HttpClient(handler), null, NullLogger<ZarinPalGateway>.Instance);

        var result = await gateway.StartAsync(new PaymentStartDto { PaymentId = 5, Amount = 50000 }, new Merchant());

        Assert.False(result.IsSuccess);
        Assert.False(handler.Called);
    }

    [Theory]
    [InlineData("?Token=999&status=0&OrderId=5&Amount=500000")]   // توکن پرداخت دیگر
    [InlineData("?Token=111&status=0&OrderId=6&Amount=500000")]   // سفارش دیگر
    [InlineData("?Token=111&status=0&OrderId=5&Amount=100000")]   // مبلغ دیگر
    [InlineData("?status=0&OrderId=5")]                            // بدون توکن
    public async Task Parsian_RejectsResultThatIsNotThisPayment(string query)
    {
        var gateway = new ParsianGateway(NullLogger<ParsianGateway>.Instance);

        var result = await gateway.CallbackAsync(Payment(5, "111"), new Merchant(), Request(query));

        Assert.False(result.IsSuccess);
        Assert.Equal(Resource.Notification.InvalidData, result.ErrorMessage);
    }

    [Fact]
    public async Task Saman_RejectsResNumOfAnotherPayment()
    {
        var gateway = new SamanKishGateway(null, null);

        var result = await gateway.CallbackAsync(Payment(5, "tok"), new Merchant(), Request("?State=OK&Status=2&RefNum=R1&ResNum=6"));

        Assert.False(result.IsSuccess);
        Assert.Equal(Resource.Notification.InvalidData, result.ErrorMessage);
    }

    [Fact]
    public async Task Saman_RejectsTokenOfAnotherPayment()
    {
        var gateway = new SamanKishGateway(null, null);

        var result = await gateway.CallbackAsync(Payment(5, "tok"), new Merchant(), Request("?State=OK&Status=2&RefNum=R1&ResNum=5&Token=other"));

        Assert.False(result.IsSuccess);
        Assert.Equal(Resource.Notification.InvalidData, result.ErrorMessage);
    }

    private sealed class FakeEnvironment : IHostEnvironment
    {
        public FakeEnvironment(string name) { EnvironmentName = name; }
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    [Theory]
    [InlineData("Production", false, false)]
    [InlineData("Production", true, true)]
    [InlineData("Development", false, true)]
    [InlineData("Staging", false, true)]
    public void TestMode_IsIgnoredInProduction_UnlessExplicitlyAllowed(string environment, bool allowInProduction, bool expected)
    {
        var service = new PaymentTestModeService(
            Options.Create(new PaymentTestModeOptions { Enabled = true, AllowInProduction = allowInProduction }),
            new FakeEnvironment(environment));

        Assert.Equal(expected, service.IsEnabled);
    }
}
