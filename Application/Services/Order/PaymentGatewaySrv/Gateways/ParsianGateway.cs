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
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Application.Services.Order.PaymentGatewaySrv.Gateways
{
    public class ParsianGateway : IPaymentGateway
    {
        public MerchantEnum Provider => MerchantEnum.parsian;

        private readonly ILogger<ParsianGateway> _logger;

        public ParsianGateway(ILogger<ParsianGateway> logger)
        {
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
                    OrderId = dto.PaymentId,
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
                    GatewayOrderId = dto.PaymentId.ToString(),
                    PaymentUrl = $"https://pec.shaparak.ir/NewIPG/?Token={result.Token}"
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
                string status = null;
                string token = null;
                string rrn = null;
                string amountStr = null;

                if (request.HasFormContentType)
                {
                    status = request.Form["status"].FirstOrDefault() ?? request.Form["Status"].FirstOrDefault();
                    token = request.Form["Token"].FirstOrDefault() ?? request.Form["token"].FirstOrDefault();
                    rrn = request.Form["RRN"].FirstOrDefault() ?? request.Form["rrn"].FirstOrDefault();
                    amountStr = request.Form["Amount"].FirstOrDefault();
                }

                status ??= request.Query["status"].FirstOrDefault() ?? request.Query["Status"].FirstOrDefault();
                token ??= request.Query["Token"].FirstOrDefault() ?? request.Query["token"].FirstOrDefault();
                rrn ??= request.Query["RRN"].FirstOrDefault() ?? request.Query["rrn"].FirstOrDefault();
                amountStr ??= request.Query["Amount"].FirstOrDefault();
                var orderId = (request.HasFormContentType ? request.Form["OrderId"].FirstOrDefault() : null)
                    ?? request.Query["OrderId"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(token))
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };

                // توکن باید همانی باشد که پارسیان موقع شروع برای همین پرداخت داد (Payment.Token)؛ وگرنه توکنِ یک پرداخت
                // ارزان‌تر روی callback پرداخت گران‌تر confirm می‌شد. پرداخت‌های قبل از این انتشار توکن ذخیره‌شده ندارند (حداکثر ۳۰ دقیقه).
                if (!string.IsNullOrWhiteSpace(payment.Token) &&
                    !string.Equals(payment.Token.Trim(), token.Trim(), StringComparison.Ordinal))
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };
                if (!string.IsNullOrWhiteSpace(orderId) && orderId.Trim() != payment.Id.ToString())
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };
                if (!string.IsNullOrWhiteSpace(amountStr) && long.TryParse(amountStr, out var callbackAmount) &&
                    callbackAmount != Convert.ToInt64(payment.Amount * 10))
                    return new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData };

                var verifyBody = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                                <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                                xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                                xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                <soap:Body>
                                    <ConfirmPayment xmlns=""https://pec.shaparak.ir/NewIPGServices/Confirm/ConfirmService"">
                                        <requestData>
                                            <LoginAccount>{merchant.Username}</LoginAccount>
                                            <Token>{token}</Token>
                                        </requestData>
                                    </ConfirmPayment>
                                </soap:Body>
                                </soap:Envelope>";

                using var client = new HttpClient();
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://pec.shaparak.ir/NewIPGServices/Confirm/ConfirmService.asmx");

                requestMessage.Content = new StringContent(verifyBody, Encoding.UTF8, "text/xml");
                requestMessage.Headers.TryAddWithoutValidation("SOAPAction", "https://pec.shaparak.ir/NewIPGServices/Confirm/ConfirmService/ConfirmPayment");

                var response = await client.SendAsync(requestMessage);
                var responseXml = await response.Content.ReadAsStringAsync();

                var doc = XDocument.Parse(responseXml);
                var resultNode = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "ConfirmPaymentResult");
                var statusNode = resultNode.Descendants().FirstOrDefault(x => x.Name.LocalName == "Status");
                var verifyStatus = statusNode?.Value?.Trim();
                if (verifyStatus != "0")
                {
                    return new GatewayCallbackResultDto
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Verify failed : {verifyStatus}"
                    };
                }
                return new GatewayCallbackResultDto
                {
                    IsSuccess = true,
                    Token = token,
                    RefNumber = rrn,
                    Description = payment.Description
                };
            }
            catch (Exception)
            {
                payment.Description = "PARSIAN_CALLBACK_FAILED";

                return new GatewayCallbackResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = Resource.Notification.Unsuccess
                };
            }
        }
    }
}
