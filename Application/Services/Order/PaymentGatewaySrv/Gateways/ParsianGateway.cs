using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.Order.PaymentGatewaySrv.Dto;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Entities.Entities;
using IPGServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Application.Services.Order.PaymentGatewaySrv.Gateways
{
    // مستندات تاپ: سند ۱۰۰۱۰۱ (خرید)، ۱۰۰۱۱۱ (تأیید)، ۱۰۰۱۲۱ (برگشت) — pgw.pec.ir/Content/Document/IPG
    public class ParsianGateway : IPaymentGateway
    {
        public MerchantEnum Provider => MerchantEnum.parsian;

        // namespace/SOAPAction طبق WSDL با S بزرگ است (pec.Shaparak.ir)؛ ASMX حساس به حروف است و با s کوچک تأیید هرگز موفق نمی‌شد
        private const string ConfirmUrl = "https://pec.shaparak.ir/NewIPGServices/Confirm/ConfirmService.asmx";
        private const string ConfirmNamespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService";
        private const string ReverseUrl = "https://pec.shaparak.ir/NewIPGServices/Reverse/ReversalService.asmx";
        private const string ReverseNamespace = "https://pec.Shaparak.ir/NewIPGServices/Reversal/ReversalService";
        // کاربر روی صفحه‌ی بانک «انصراف» زد
        private const string CancelledStatus = "-138";

        // OrderId نزد پارسیان یکتا کنترل می‌شود (‎-112 = تکراری). Payment.Id خام با شناسه‌هایی که قبلاً روی همین پذیرنده
        // فرستاده شده بود (نسخه‌ی قبلی/پایانه‌ی تست) برخورد کرد؛ این پایه آن را به بازه‌ای می‌برد که پیش‌تر استفاده نشده.
        public const long OrderIdBase = 7_000_000_000_000;

        public static long ToOrderId(long paymentId) => OrderIdBase + paymentId;

        // طبق مستند، خطای ارتباطی در تأیید/برگشت باید دست‌کم یک‌بار تکرار شود؛ بعد از آن جواب قطعی نداریم (IsPending)
        private static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(20);
        private const int MaxAttempts = 2;

        private readonly HttpClient _httpClient;
        private readonly ILogger<ParsianGateway> _logger;

        public ParsianGateway(HttpClient httpClient, ILogger<ParsianGateway> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<GatewayStartResultDto> StartAsync(PaymentStartDto dto, Merchant merchant)
        {
            try
            {
                var client = new SaleServiceSoapClient(SaleServiceSoapClient.EndpointConfiguration.SaleServiceSoap);
                var request = new IPGServices.ClientSaleRequestData
                {
                    LoginAccount = merchant.Username,
                    Amount = Convert.ToInt64(dto.Amount * 10),
                    OrderId = ToOrderId(dto.PaymentId),
                    CallBackUrl = dto.CallbackUrl
                };

                var response = await client.SalePaymentRequestAsync(request);

                var result = response.Body?.SalePaymentRequestResult;

                if (result == null)
                    return Fail(Resource.Notification.EmptyResponseFromParsian);

                if (result.Status != 0)
                    return Fail($"Parsian Error - Status:{result.Status} Message:{result.Message}");

                if (result.Token <= 0)
                    return Fail(Resource.Notification.InvalidTokenFromParsian);

                return new GatewayStartResultDto
                {
                    IsSuccess = true,
                    PaymentIsLink = true,
                    Token = result.Token.ToString(),
                    GatewayOrderId = ToOrderId(dto.PaymentId).ToString(),
                    PaymentUrl = $"https://pec.shaparak.ir/NewIPG/?token={result.Token}"
                };
            }
            catch (Exception ex)
            {
                // جزئیات فقط در لاگ؛ متن exception (با stack trace) به کاربر برنمی‌گردد
                _logger.LogError(ex, "Parsian start failed for payment {PaymentId}.", dto.PaymentId);
                return Fail(Resource.Notification.ErrorOnStartPayment);
            }
        }

        private static GatewayStartResultDto Fail(string message)
        {
            return new GatewayStartResultDto
            {
                IsSuccess = false,
                ErrorMessage = message
            };
        }

        public async Task<GatewayCallbackResultDto> CallbackAsync(Payment payment, Merchant merchant, HttpRequest request)
        {
            try
            {
                // بانک نتیجه را form-urlencoded با POST می‌فرستد؛ query فقط برای سازگاری
                var status = Field(request, "status", "Status");
                var token = Field(request, "Token", "token");
                var rrn = Field(request, "RRN", "rrn");
                var amountStr = Field(request, "Amount");
                var orderId = Field(request, "OrderId");

                if (string.IsNullOrWhiteSpace(token))
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };

                // توکن باید همانی باشد که پارسیان موقع شروع برای همین پرداخت داد (Payment.Token)؛ وگرنه توکنِ یک پرداخت
                // ارزان‌تر روی callback پرداخت گران‌تر confirm می‌شد. پرداخت‌های قبل از این انتشار توکن ذخیره‌شده ندارند (حداکثر ۳۰ دقیقه).
                if (!string.IsNullOrWhiteSpace(payment.Token) &&
                    !string.Equals(payment.Token.Trim(), token.Trim(), StringComparison.Ordinal))
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };
                // شکل قدیمی (Payment.Id خام) هم برای پرداخت‌های شروع‌شده قبل از این انتشار پذیرفته می‌شود
                if (!string.IsNullOrWhiteSpace(orderId) &&
                    orderId.Trim() != ToOrderId(payment.Id).ToString() && orderId.Trim() != payment.Id.ToString())
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };
                // Amount با جداکننده‌ی هزارگان می‌آید ("10,000")؛ قبلاً parse نمی‌شد و این بررسی بی‌صدا رد می‌شد.
                // قالب ناشناخته رد نمی‌کند (بایندِ توکن کافی است) ولی لاگ می‌شود تا قالب واقعی دیده شود.
                if (!string.IsNullOrWhiteSpace(amountStr))
                {
                    if (!TryParseAmount(amountStr, out var callbackAmount))
                        _logger.LogWarning("Parsian callback for payment {PaymentId} has an unparseable Amount '{Amount}'.", payment.Id, amountStr);
                    else if (callbackAmount != Convert.ToInt64(payment.Amount * 10))
                        return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };
                }
                if (!long.TryParse(token.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var tokenValue) || tokenValue <= 0)
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };

                // تراکنش فقط با status=0 و RRN>0 موفق است؛ در غیر این صورت (از جمله ‎-138 انصراف) نباید تأیید زده شود
                var paid = status?.Trim() == "0" &&
                           long.TryParse(rrn?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var rrnValue) && rrnValue > 0;
                if (!paid)
                {
                    return new GatewayCallbackResultDto
                    {
                        IsSuccess = false,
                        Token = token,
                        Description = status?.Trim() == CancelledStatus ? "PARSIAN_CANCELLED" : $"PARSIAN_STATUS:{status}",
                        ErrorMessage = Resource.Notification.Unsuccess
                    };
                }

                var confirm = await CallAsync(ConfirmUrl, ConfirmNamespace, "ConfirmPayment", merchant.Username, tokenValue, payment.Id);
                if (confirm.Status == null)
                {
                    // جواب قطعی نرسید: پول ممکن است قطعی شده باشد؛ «ناموفق» ثبت نمی‌شود تا رفرش callback دوباره تأیید را امتحان کند
                    return new GatewayCallbackResultDto
                    {
                        IsSuccess = false,
                        IsPending = true,
                        Token = token,
                        ErrorMessage = Resource.Notification.PaymentVerificationPending
                    };
                }
                if (confirm.Status != "0")
                {
                    return new GatewayCallbackResultDto
                    {
                        IsSuccess = false,
                        Token = token,
                        Description = $"PARSIAN_CONFIRM:{confirm.Status}",
                        ErrorMessage = Resource.Notification.Unsuccess
                    };
                }
                return new GatewayCallbackResultDto
                {
                    IsSuccess = true,
                    Token = token,
                    RefNumber = string.IsNullOrWhiteSpace(confirm.Rrn) || confirm.Rrn == "0" ? rrn : confirm.Rrn,
                    Description = payment.Description
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parsian callback failed for payment {PaymentId}.", payment?.Id);
                return new GatewayCallbackResultDto
                {
                    IsSuccess = false,
                    Description = "PARSIAN_CALLBACK_FAILED",
                    ErrorMessage = Resource.Notification.Unsuccess
                };
            }
        }

        // برگشت وجه تراکنشِ تأییدشده‌ای که خدمتش ارائه نمی‌شود (سند ۱۰۰۱۲۱). فقط قبل از پایان روز مالی شاپرک ممکن است.
        //  IsSuccess = برگشت انجام شد؛ IsPending = جواب قطعی نرسید (ممکن است انجام شده باشد، نباید دوباره پول داد)؛
        //  هیچ‌کدام = بانک قطعاً رد کرد و پول نزد ما است.
        public async Task<GatewayCallbackResultDto> ReverseAsync(Payment payment, Merchant merchant)
        {
            var token = payment?.Token?.Trim();
            if (!long.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out var tokenValue) || tokenValue <= 0)
                return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };

            var result = await CallAsync(ReverseUrl, ReverseNamespace, "ReversalRequest", merchant.Username, tokenValue, payment.Id);
            if (result.Status == null)
                return new GatewayCallbackResultDto { IsSuccess = false, IsPending = true, Token = token };
            return new GatewayCallbackResultDto
            {
                IsSuccess = result.Status == "0",
                Token = token,
                Description = $"PARSIAN_REVERSE:{result.Status}",
                ErrorMessage = result.Status == "0" ? null : Resource.Notification.Unsuccess
            };
        }

        // Status == null یعنی جواب قطعی نیامد (تایم‌اوت، خطای شبکه، SOAP fault، پاسخ نامعتبر)
        private async Task<(string Status, string Rrn)> CallAsync(string url, string ns, string method, string loginAccount, long token, long paymentId)
        {
            var resultName = method == "ReversalRequest" ? "ReversalRequestResult" : method + "Result";
            var body = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <{method} xmlns=""{ns}"">
      <requestData>
        <LoginAccount>{SecurityElement.Escape(loginAccount)}</LoginAccount>
        <Token>{token}</Token>
      </requestData>
    </{method}>
  </soap:Body>
</soap:Envelope>";

            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    using var timeout = new CancellationTokenSource(AttemptTimeout);
                    using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                    requestMessage.Content = new StringContent(body, Encoding.UTF8, "text/xml");
                    requestMessage.Headers.TryAddWithoutValidation("SOAPAction", $"\"{ns}/{method}\"");

                    using var response = await _httpClient.SendAsync(requestMessage, timeout.Token);
                    var responseXml = await response.Content.ReadAsStringAsync(timeout.Token);

                    var resultNode = TryParse(responseXml)?.Descendants().FirstOrDefault(x => x.Name.LocalName == resultName);
                    var status = resultNode?.Elements().FirstOrDefault(x => x.Name.LocalName == "Status")?.Value?.Trim();
                    if (!string.IsNullOrEmpty(status))
                    {
                        var rrn = resultNode.Elements().FirstOrDefault(x => x.Name.LocalName == "RRN")?.Value?.Trim();
                        if (status != "0")
                            _logger.LogWarning("Parsian {Method} for payment {PaymentId} returned status {Status}.", method, paymentId, status);
                        return (status, rrn);
                    }

                    _logger.LogWarning("Parsian {Method} for payment {PaymentId} gave no definitive answer (attempt {Attempt}, HTTP {Http}).",
                        method, paymentId, attempt, (int)response.StatusCode);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Parsian {Method} for payment {PaymentId} failed (attempt {Attempt}).", method, paymentId, attempt);
                }
            }
            return (null, null);
        }

        private static XDocument TryParse(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return null;
            try { return XDocument.Parse(xml); }
            catch (System.Xml.XmlException) { return null; }
        }

        private static string Field(HttpRequest request, params string[] names)
        {
            if (request.HasFormContentType)
            {
                foreach (var name in names)
                {
                    var value = request.Form[name].FirstOrDefault();
                    if (value != null)
                        return value;
                }
            }
            foreach (var name in names)
            {
                var value = request.Query[name].FirstOrDefault();
                if (value != null)
                    return value;
            }
            return null;
        }

        // "10,000" / "10٬000" / "10,000.00" / "10000" → 10000 (ریال)
        public static bool TryParseAmount(string value, out long amount)
        {
            amount = 0;
            var normalized = new string(value.Where(c => c != '٬' && c != '،' && !char.IsWhiteSpace(c)).ToArray());
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ||
                parsed != decimal.Truncate(parsed))
                return false;
            amount = (long)parsed;
            return true;
        }
    }
}
