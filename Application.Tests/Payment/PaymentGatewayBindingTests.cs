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
        var handler = new NoNetworkHandler();
        var gateway = new ParsianGateway(new HttpClient(handler), NullLogger<ParsianGateway>.Instance);

        var result = await gateway.CallbackAsync(Payment(5, "111"), new Merchant(), Request(query));

        Assert.False(result.IsSuccess);
        Assert.Equal(Resource.Notification.InvalidData, result.ErrorMessage);
        Assert.False(handler.Called);
    }

    // پاسخ ساختگی SOAP پارسیان؛ هر درخواست ثبت می‌شود
    private sealed class SoapHandler : HttpMessageHandler
    {
        private readonly Func<HttpResponseMessage> _respond;
        public SoapHandler(Func<HttpResponseMessage> respond) { _respond = respond; }
        public int Calls { get; private set; }
        public string LastSoapAction { get; private set; }
        public string LastBody { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            LastSoapAction = string.Join("", request.Headers.GetValues("SOAPAction"));
            LastBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return _respond();
        }
    }

    private static HttpResponseMessage Soap(string result, string fields) => new(System.Net.HttpStatusCode.OK)
    {
        Content = new StringContent(
            $"<?xml version=\"1.0\" encoding=\"utf-8\"?><soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><{result}Response xmlns=\"https://pec.Shaparak.ir/x\"><{result}Result>{fields}</{result}Result></{result}Response></soap:Body></soap:Envelope>",
            System.Text.Encoding.UTF8, "text/xml")
    };

    [Theory]
    [InlineData("?Token=111&status=-138&OrderId=5")]                       // انصراف کاربر
    [InlineData("?Token=111&status=0&OrderId=5&RRN=0&Amount=500,000")]     // بدون RRN
    [InlineData("?Token=111&status=-1&OrderId=5&RRN=77&Amount=500,000")]   // تراکنش ناموفق
    public async Task Parsian_DoesNotConfirm_UnlessStatusZeroAndRrnPositive(string query)
    {
        var handler = new NoNetworkHandler();
        var gateway = new ParsianGateway(new HttpClient(handler), NullLogger<ParsianGateway>.Instance);

        var result = await gateway.CallbackAsync(Payment(5, "111"), new Merchant { Username = "pin" }, Request(query));

        Assert.False(result.IsSuccess);
        Assert.False(result.IsPending);
        Assert.False(handler.Called);
    }

    [Theory]
    [InlineData("500,000")]
    [InlineData("500000")]
    [InlineData("500,000.00")]
    public async Task Parsian_ConfirmsPaidResult_WithFormattedAmount_AndWsdlNamespace(string amount)
    {
        var handler = new SoapHandler(() => Soap("ConfirmPayment", "<Status>0</Status><CardNumberMasked>6219***1234</CardNumberMasked><RRN>9876</RRN><Token>111</Token>"));
        var gateway = new ParsianGateway(new HttpClient(handler), NullLogger<ParsianGateway>.Instance);

        var result = await gateway.CallbackAsync(Payment(5, "111"), new Merchant { Username = "pin" },
            Request($"?Token=111&status=0&OrderId=5&RRN=9876&Amount={Uri.EscapeDataString(amount)}"));

        Assert.True(result.IsSuccess);
        Assert.Equal("9876", result.RefNumber);
        Assert.Equal(1, handler.Calls);
        Assert.Equal("\"https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService/ConfirmPayment\"", handler.LastSoapAction);
        Assert.Contains("<ConfirmPayment xmlns=\"https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService\">", handler.LastBody);
        Assert.Contains("<Token>111</Token>", handler.LastBody);
    }

    [Theory]
    [InlineData("7000000000005")]   // شکل تازه: OrderIdBase + Payment.Id
    [InlineData("5")]               // شکل قدیمی (پرداخت‌های قبل از انتشار)
    public async Task Parsian_AcceptsOffsetOrderIdOfThisPayment(string orderId)
    {
        var handler = new SoapHandler(() => Soap("ConfirmPayment", "<Status>0</Status><RRN>9876</RRN><Token>111</Token>"));
        var gateway = new ParsianGateway(new HttpClient(handler), NullLogger<ParsianGateway>.Instance);

        var result = await gateway.CallbackAsync(Payment(5, "111"), new Merchant { Username = "pin" },
            Request($"?Token=111&status=0&OrderId={orderId}&RRN=9876"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Parsian_RejectsOffsetOrderIdOfAnotherPayment()
    {
        var handler = new NoNetworkHandler();
        var gateway = new ParsianGateway(new HttpClient(handler), NullLogger<ParsianGateway>.Instance);

        var result = await gateway.CallbackAsync(Payment(5, "111"), new Merchant(), Request("?Token=111&status=0&OrderId=7000000000006&RRN=9"));

        Assert.False(result.IsSuccess);
        Assert.Equal(Resource.Notification.InvalidData, result.ErrorMessage);
        Assert.False(handler.Called);
    }

    [Fact]
    public void Parsian_OrderId_IsOffsetFromPaymentId()
    {
        Assert.Equal(7_000_000_000_042L, ParsianGateway.ToOrderId(42));
    }

    [Fact]
    public async Task Parsian_RejectsFormattedAmountOfAnotherPrice()
    {
        var handler = new NoNetworkHandler();
        var gateway = new ParsianGateway(new HttpClient(handler), NullLogger<ParsianGateway>.Instance);

        var result = await gateway.CallbackAsync(Payment(5, "111"), new Merchant(), Request("?Token=111&status=0&OrderId=5&RRN=9&Amount=10%2C000"));

        Assert.False(result.IsSuccess);
        Assert.Equal(Resource.Notification.InvalidData, result.ErrorMessage);
        Assert.False(handler.Called);
    }

    [Fact]
    public async Task Parsian_ConfirmNetworkFailure_IsRetriedOnce_ThenPending()
    {
        var handler = new SoapHandler(() => throw new HttpRequestException("down"));
        var gateway = new ParsianGateway(new HttpClient(handler), NullLogger<ParsianGateway>.Instance);

        var result = await gateway.CallbackAsync(Payment(5, "111"), new Merchant { Username = "pin" }, Request("?Token=111&status=0&OrderId=5&RRN=9"));

        Assert.False(result.IsSuccess);
        Assert.True(result.IsPending);
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task Parsian_ConfirmRejected_IsDefinitiveFailure()
    {
        var handler = new SoapHandler(() => Soap("ConfirmPayment", "<Status>-1533</Status><RRN>0</RRN><Token>111</Token>"));
        var gateway = new ParsianGateway(new HttpClient(handler), NullLogger<ParsianGateway>.Instance);

        var result = await gateway.CallbackAsync(Payment(5, "111"), new Merchant { Username = "pin" }, Request("?Token=111&status=0&OrderId=5&RRN=9"));

        Assert.False(result.IsSuccess);
        Assert.False(result.IsPending);
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task Parsian_Reverse_Success_UsesReversalService()
    {
        var handler = new SoapHandler(() => Soap("ReversalRequest", "<Status>0</Status><Message>ok</Message><Token>111</Token>"));
        var gateway = new ParsianGateway(new HttpClient(handler), NullLogger<ParsianGateway>.Instance);

        var result = await gateway.ReverseAsync(Payment(5, "111"), new Merchant { Username = "pin" });

        Assert.True(result.IsSuccess);
        Assert.Equal("\"https://pec.Shaparak.ir/NewIPGServices/Reversal/ReversalService/ReversalRequest\"", handler.LastSoapAction);
    }

    [Fact]
    public async Task Parsian_Reverse_Refused_IsNotPending()
    {
        var handler = new SoapHandler(() => Soap("ReversalRequest", "<Status>-1</Status><Message>no</Message><Token>111</Token>"));
        var gateway = new ParsianGateway(new HttpClient(handler), NullLogger<ParsianGateway>.Instance);

        var result = await gateway.ReverseAsync(Payment(5, "111"), new Merchant { Username = "pin" });

        Assert.False(result.IsSuccess);
        Assert.False(result.IsPending);
    }

    [Fact]
    public async Task Parsian_Reverse_NoAnswer_IsPending()
    {
        var handler = new SoapHandler(() => new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { Content = new StringContent("") });
        var gateway = new ParsianGateway(new HttpClient(handler), NullLogger<ParsianGateway>.Instance);

        var result = await gateway.ReverseAsync(Payment(5, "111"), new Merchant { Username = "pin" });

        Assert.False(result.IsSuccess);
        Assert.True(result.IsPending);
        Assert.Equal(2, handler.Calls);
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
