using Application.Services.Order.PaymentGatewaySrv.Dto;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Common.Enumerable;
using Application.Common.Helpers.Iface;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentGatewaySrv.Gateways
{
    public class ZarinPalGateway : IPaymentGateway
    {
        public MerchantEnum Provider => MerchantEnum.zarinpal;

        private readonly HttpClient _httpClient;
        private readonly IAdminSettingHelper _adminSettingHelper;

        private const string ZarinPalApiUrl = "https://api.zarinpal.com/pg/v4/payment/request.json";
        private const string ZarinPalVerificationUrl = "https://api.zarinpal.com/pg/v4/payment/verify.json";

        private readonly ILogger<ZarinPalGateway> _logger;

        public ZarinPalGateway(HttpClient httpClient, IAdminSettingHelper adminSettingHelper, ILogger<ZarinPalGateway> logger)
        {
            _logger = logger;
            _httpClient = httpClient;
            _adminSettingHelper = adminSettingHelper;
        }

        public async Task<GatewayStartResultDto> StartAsync(PaymentStartDto dto, Merchant merchant)
        {
            try
            {
                // آدرس برگشت باید callbackToken همین پرداخت را داشته باشد (قبلاً از تنظیم ادمین PaymentUrl + id ساخته می‌شد و
                // callback بدون توکن همیشه رد می‌شد)
                if (string.IsNullOrWhiteSpace(dto.CallbackUrl))
                    return new GatewayStartResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.ErrorOnStartPayment };

                var requestData = new
                {
                    merchant_id = merchant.MerchantNo,
                    amount = Convert.ToInt32(dto.Amount * 10),
                    callback_url = dto.CallbackUrl,
                    description = string.Format(Resource.Pattern.PaymentDescription, dto.ProductOrderId),
                    metadata = new { email = dto.User?.Email, mobile = dto.User?.Mobile }
                };

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(ZarinPalApiUrl, content);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode || !result.Contains("\"authority\""))
                {
                    _logger.LogError("ZarinPal request failed. HTTP {Status}. Body: {Body}", (int)response.StatusCode, result);
                    return new GatewayStartResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.ErrorOnStartPayment };
                }

                var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<ZarinPalPaymentResponse>(result);

                return new GatewayStartResultDto
                {
                    IsSuccess = true,
                    PaymentIsLink = true,
                    PaymentUrl = $"https://www.zarinpal.com/pg/StartPay/{jsonResponse.data.authority}",
                    Token = jsonResponse.data.authority
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ZarinPal start threw an exception");
                return new GatewayStartResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.ErrorOnStartPayment };
            }
        }

        public async Task<GatewayCallbackResultDto> CallbackAsync(Payment payment, Merchant merchant, HttpRequest request)
        {
            try
            {
                var status = request.Query["Status"].ToString();
                var authority = request.Query["Authority"].ToString();

                if (string.IsNullOrEmpty(status) || string.IsNullOrEmpty(authority))
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };

                // NOK = کاربر «انصراف» زد یا بانک تراکنش را رد کرد؛ verify نمی‌شود و کاربر دلیل را روی صفحه‌ی نتیجه می‌بیند
                if (!string.Equals(status.Trim(), "OK", StringComparison.OrdinalIgnoreCase))
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.PaymentNotCompletedAtGateway, Description = "GATEWAY_NOK" };

                // Authority باید همانی باشد که زرین‌پال موقع شروع برای همین پرداخت داد؛ Authority پرداخت دیگری verify نمی‌شود
                if (string.IsNullOrWhiteSpace(payment.Token) ||
                    !string.Equals(payment.Token.Trim(), authority.Trim(), StringComparison.Ordinal))
                {
                    _logger.LogWarning("ZarinPal callback authority does not match payment {PaymentId}.", payment.Id);
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };
                }

                return await VerifyStoredAuthorityAsync(payment, merchant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ZarinPal callback threw for payment {PaymentId}.", payment?.Id);
                return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.Unsuccess };
            }
        }

        // verify با Authority ذخیره‌شده‌ی همین پرداخت و مبلغ همین پرداخت. هم callback از این استفاده می‌کند و هم job تطبیق
        // (پرداخت‌هایی که کاربر در درگاه پرداخت کرد ولی مرورگرش هرگز به callback برنگشت).
        //  - ۱۰۰ = موفق؛ ۱۰۱ = «قبلاً verify شده» برای همین Authority و همین مبلغ، یعنی پرداخت واقعی است (یک verify قبلی که
        //    جوابش به ما نرسید یا قبل از ذخیره قطع شد) و موفق حساب می‌شود؛ اعمال روی سفارش خودش یک‌بار‌مصرف است.
        //  - پاسخ قطعی منفی (پرداخت نشده/ناموفق/مبلغ نامطابق) = ناموفق.
        //  - تایم‌اوت، خطای شبکه، HTTP 5xx/429 یا پاسخ نامعتبر = IsPending: پرداخت «ناموفق» ثبت نمی‌شود (قبلاً می‌شد و پولِ
        //    کسرشده هرگز به سفارش نمی‌رسید). هر تلاش ۲۰ ثانیه سقف دارد و یک‌بار تکرار می‌شود (verify تکراری ۱۰۱ می‌دهد، بی‌خطر).
        private static readonly TimeSpan VerifyAttemptTimeout = TimeSpan.FromSeconds(20);
        private const int VerifyMaxAttempts = 2;

        public async Task<GatewayCallbackResultDto> VerifyStoredAuthorityAsync(Payment payment, Merchant merchant)
        {
            var authority = payment?.Token?.Trim();
            if (string.IsNullOrEmpty(authority))
                return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };

            var verifyData = new
            {
                merchant_id = merchant.MerchantNo,
                authority = authority,
                amount = Convert.ToInt32(payment.Amount * 10),
            };
            var body = System.Text.Json.JsonSerializer.Serialize(verifyData);

            for (var attempt = 1; attempt <= VerifyMaxAttempts; attempt++)
            {
                try
                {
                    using var timeout = new System.Threading.CancellationTokenSource(VerifyAttemptTimeout);
                    using var content = new StringContent(body, Encoding.UTF8, "application/json");
                    using var response = await _httpClient.PostAsync(ZarinPalVerificationUrl, content, timeout.Token);
                    var result = await response.Content.ReadAsStringAsync(timeout.Token);

                    var outcome = ParseVerifyResponse(result);
                    if (outcome.Code == 100 || outcome.Code == 101)
                    {
                        return new GatewayCallbackResultDto
                        {
                            IsSuccess = true,
                            RefNumber = outcome.RefId?.ToString("0"),
                            Token = authority,
                            Description = $"{outcome.Code}--{outcome.RefId:0}"
                        };
                    }
                    var httpStatus = (int)response.StatusCode;
                    var transientHttp = httpStatus >= 500 || httpStatus == 429;
                    if (outcome.Code.HasValue && !transientHttp)
                    {
                        return new GatewayCallbackResultDto
                        {
                            IsSuccess = false,
                            Token = authority,
                            Description = outcome.Code.Value.ToString(),
                            ErrorMessage = Resource.Notification.Unsuccess
                        };
                    }

                    _logger.LogWarning("ZarinPal verify for payment {PaymentId} gave no definitive answer (attempt {Attempt}, HTTP {Status}).",
                        payment.Id, attempt, (int)response.StatusCode);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "ZarinPal verify for payment {PaymentId} failed (attempt {Attempt}).", payment?.Id, attempt);
                }
            }

            return new GatewayCallbackResultDto
            {
                IsSuccess = false,
                IsPending = true,
                Token = authority,
                ErrorMessage = Resource.Notification.PaymentVerificationPending
            };
        }

        // پاسخ موفق: {"data":{"code":100,"ref_id":...},"errors":[]}؛ پاسخ خطا: {"data":[],"errors":{"code":-53,...}}
        private static (int? Code, double? RefId) ParseVerifyResponse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return (null, null);
            try
            {
                using var document = System.Text.Json.JsonDocument.Parse(json);
                var root = document.RootElement;
                if (root.ValueKind != System.Text.Json.JsonValueKind.Object)
                    return (null, null);

                if (root.TryGetProperty("data", out var data) && data.ValueKind == System.Text.Json.JsonValueKind.Object &&
                    data.TryGetProperty("code", out var dataCode) && dataCode.TryGetInt32(out var code))
                {
                    double? refId = data.TryGetProperty("ref_id", out var refElement) && refElement.TryGetDouble(out var parsedRef) ? parsedRef : null;
                    return (code, refId);
                }

                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == System.Text.Json.JsonValueKind.Object &&
                    errors.TryGetProperty("code", out var errorCode) && errorCode.TryGetInt32(out var negative))
                    return (negative, null);
            }
            catch (System.Text.Json.JsonException)
            {
            }
            return (null, null);
        }
    }
}
