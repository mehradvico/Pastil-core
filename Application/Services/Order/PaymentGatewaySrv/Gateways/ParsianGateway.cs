using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.Order.PaymentGatewaySrv.Dto;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.ProductOrderSrv.Dto;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
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

        public async Task<GatewayStartResultDto> StartAsync(
            PaymentStartDto dto,
            Merchant merchant)
        {
            try
            {
                using var client = new HttpClient();

                var soapBody = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <SalePaymentRequest xmlns=""https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService"">
<requestData>
        <LoginAccount>{merchant.Username}</LoginAccount>
        <Amount>{Convert.ToInt64(dto.Amount)}</Amount>
        <OrderId>{dto.PaymentId}</OrderId>
        <CallBackUrl>{dto.CallbackUrl}</CallBackUrl>
        <AdditionalData></AdditionalData>
      </requestData>
    </SalePaymentRequest>
  </soap:Body>
</soap:Envelope>";

                var content = new StringContent(
                    soapBody,
                    Encoding.UTF8,
                    "text/xml"
                );

                content.Headers.Add(
                    "SOAPAction",
                    "\"https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService/SalePaymentRequest\""
                );

                var response = await client.PostAsync(
                    "https://pec.shaparak.ir/NewIPGServices/Sale/SaleService.asmx",
                    content
                );

                var xml = await response.Content.ReadAsStringAsync();
                Console.WriteLine(xml);

                var doc = XDocument.Parse(xml);

                var resultNode = doc
                    .Descendants()
                    .FirstOrDefault(x =>
                        x.Name.LocalName == "SalePaymentRequestResult");

                if (resultNode == null)
                {
                    return new GatewayStartResultDto
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid bank response"
                    };
                }

                var result = resultNode.Value;

                var parts = result.Split(',');

                if (parts.Length < 2)
                {
                    return new GatewayStartResultDto
                    {
                        IsSuccess = false,
                        ErrorMessage = result
                    };
                }

                var status = parts[0];

                if (status != "0")
                {
                    return new GatewayStartResultDto
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Parsian Error: {status}"
                    };
                }

                var token = parts[1];

                return new GatewayStartResultDto
                {
                    IsSuccess = true,
                    PaymentIsLink = true,
                    PaymentUrl =
                        $"{merchant.Bank.PaymentUrl}?Token={token}",
                    Token = token,
                    GatewayOrderId = dto.PaymentId.ToString()
                };
            }
            catch (Exception ex)
            {
                return new GatewayStartResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public Task<GatewayCallbackResultDto> CallbackAsync(
            Payment payment,
            Merchant merchant,
            HttpRequest request,
            bool testMode)
        {
            var status =
                HttpRequestParamReaderHelper.Get(request, "status")
                ?? HttpRequestParamReaderHelper.Get(request, "Status");

            var token =
                HttpRequestParamReaderHelper.Get(request, "Token");

            var rrn =
                HttpRequestParamReaderHelper.Get(request, "RRN");

            if (status != "0")
            {
                return Task.FromResult(new GatewayCallbackResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"Bank Status: {status}",
                    Token = token
                });
            }

            return Task.FromResult(new GatewayCallbackResultDto
            {
                IsSuccess = true,
                RefNumber = rrn,
                Token = token,
                Description = "SUCCESS"
            });
        }
    }
}