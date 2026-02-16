using Application.Services.Order.PaymentGatewaySrv.Dto;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Common.Enumerable;
using Application.Common.Helpers.Iface;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Text;
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

        public ZarinPalGateway(HttpClient httpClient, IAdminSettingHelper adminSettingHelper)
        {
            _httpClient = httpClient;
            _adminSettingHelper = adminSettingHelper;
        }

        public async Task<GatewayStartResultDto> StartAsync(PaymentStartDto dto, Merchant merchant)
        {
            try
            {
                var requestData = new
                {
                    merchant_id = merchant.MerchantNo,
                    amount = Convert.ToInt32(dto.Amount * 10),
                    callback_url = _adminSettingHelper.BaseAdminSetting.PaymentUrl + dto.PaymentId,
                    description = string.Format(Resource.Pattern.PaymentDescription, dto.ProductOrderId),
                    metadata = new { email = dto.User?.Email, mobile = dto.User?.Mobile }
                };

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(ZarinPalApiUrl, content);
                var result = await response.Content.ReadAsStringAsync();

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
                return new GatewayStartResultDto { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<GatewayCallbackResultDto> CallbackAsync(Payment payment, Merchant merchant, HttpRequest request, bool testMode)
        {
            try
            {
                if (testMode)
                    return new GatewayCallbackResultDto { IsSuccess = true, Description = "TEST_MODE" };

                var status = request.Query["Status"].ToString();
                var authority = request.Query["Authority"].ToString();

                if (string.IsNullOrEmpty(status) || string.IsNullOrEmpty(authority))
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };

                if (!string.Equals(status.Trim(), "OK", StringComparison.OrdinalIgnoreCase))
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.Unsuccess };

                var verifyData = new
                {
                    merchant_id = merchant.MerchantNo,
                    authority = authority,
                    amount = Convert.ToInt32(payment.Amount * 10),
                };

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(verifyData),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(ZarinPalVerificationUrl, content);
                var result = await response.Content.ReadAsStringAsync();

                var verifyResponse = System.Text.Json.JsonSerializer.Deserialize<ZarinPalVerificationResponse>(result);

                if (verifyResponse?.data?.code == 100)
                {
                    return new GatewayCallbackResultDto
                    {
                        IsSuccess = true,
                        RefNumber = verifyResponse.data.ref_id.ToString(),
                        Token = authority,
                        Description = $"{verifyResponse.data.code}--{verifyResponse.data.ref_id}"
                    };
                }

                return new GatewayCallbackResultDto
                {
                    IsSuccess = false,
                    Token = authority,
                    Description = verifyResponse?.data?.code.ToString(),
                    ErrorMessage = Resource.Notification.Unsuccess
                };
            }
            catch (Exception ex)
            {
                return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }
    }
}
