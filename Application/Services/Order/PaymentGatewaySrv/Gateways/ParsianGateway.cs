using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.Order.PaymentGatewaySrv.Dto;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.ProductOrderSrv.Dto;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentGatewaySrv.Gateways
{
    public class ParsianGateway : IPaymentGateway
    {
        public MerchantEnum Provider => MerchantEnum.parsian;

        public ParsianGateway()
        {
        }

        public Task<GatewayStartResultDto> StartAsync(PaymentStartDto dto, Merchant merchant)
        {
            // TODO: وقتی کد/مستند پارسیان رو دادی، اینجا Token یا PaymentUrl ساخته میشه.
            // پارسیان معمولاً: گرفتن Token -> redirect به صفحه پرداخت با token

            // فعلاً تستی:
            return Task.FromResult(new GatewayStartResultDto
            {
                IsSuccess = true,
                PaymentIsLink = true,
                PaymentUrl = $"https://test-gateway.local/parsian/pay?paymentId={dto.PaymentId}",
                Token = dto.PaymentId.ToString(),
                GatewayOrderId = dto.PaymentId.ToString()
            });
        }

        public Task<GatewayCallbackResultDto> CallbackAsync(Payment payment, Merchant merchant, HttpRequest request, bool testMode)
        {
            if (testMode)
            {
                return Task.FromResult(new GatewayCallbackResultDto
                {
                    IsSuccess = true,
                    RefNumber = $"TEST-PARSIAN-{payment.Id}",
                    Description = "TEST_MODE"
                });
            }

            // پارسیان معمولاً callback params مثل: Token / Status / RRN / ... دارد.
            // اینجا هم Query و هم Form باید خوانده شود.

            var status = HttpRequestParamReaderHelper.Get(request, "status") ?? HttpRequestParamReaderHelper.Get(request, "Status");
            var token = HttpRequestParamReaderHelper.Get(request, "token") ?? HttpRequestParamReaderHelper.Get(request, "Token");
            var rrn = HttpRequestParamReaderHelper.Get(request, "RRN") ?? HttpRequestParamReaderHelper.Get(request, "rrn");

            // TODO: Verify واقعی با سرویس پارسیان

            if (string.IsNullOrEmpty(status))
                return Task.FromResult(new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData });

            // این شرط فقط placeholder است تا وقتی mapping دقیق کدها را آوردی
            var ok = status == "0" || status.Equals("OK", StringComparison.OrdinalIgnoreCase);

            return Task.FromResult(new GatewayCallbackResultDto
            {
                IsSuccess = ok,
                RefNumber = rrn,
                Token = token,
                Description = $"status={status}"
            });
        }
    }
}
