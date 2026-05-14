using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.Order.PaymentGatewaySrv.Dto;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.ProductOrderSrv.Dto;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentGatewaySrv.Gateways
{
    public class MellatGateway : IPaymentGateway
    {
        public MerchantEnum Provider => MerchantEnum.mellat;

        public MellatGateway()
        {
        }

        public Task<GatewayStartResultDto> StartAsync(PaymentStartDto dto, Merchant merchant)
        {
            // TODO: ملت SOAP است: bpPayRequest
            // خروجی: RefId و بعد باید کاربر را به صفحه پرداخت ملت POST کنی.

            // فعلاً تستی:
            return Task.FromResult(new GatewayStartResultDto
            {
                IsSuccess = true,
                PaymentIsLink = false,
                HtmlForm =
                    $"<form id='f' action='https://test-gateway.local/mellat/pay' method='post'>" +
                    $"<input type='hidden' name='RefId' value='{dto.PaymentId}' />" +
                    $"</form><script>document.getElementById('f').submit();</script>",
                Token = dto.PaymentId.ToString(),
                GatewayOrderId = dto.PaymentId.ToString()
            });
        }

        public Task<GatewayCallbackResultDto> CallbackAsync(Payment payment, Merchant merchant, HttpRequest request/*, bool testMode*/)
        {
            //if (testMode)
            //{
            //    return Task.FromResult(new GatewayCallbackResultDto
            //    {
            //        IsSuccess = true,
            //        RefNumber = $"TEST-MELLAT-{payment.Id}",
            //        Description = "TEST_MODE"
            //    });
            //}

            // ملت معمولاً POST برمی‌گرداند: SaleOrderId, SaleReferenceId, RefId, ResCode ...
            var resCode = HttpRequestParamReaderHelper.Get(request, "ResCode");
            var saleOrderId = HttpRequestParamReaderHelper.Get(request, "SaleOrderId");
            var saleReferenceId = HttpRequestParamReaderHelper.Get(request, "SaleReferenceId");

            if (string.IsNullOrEmpty(resCode))
                return Task.FromResult(new GatewayCallbackResultDto { IsSuccess = false, ErrorMessage = Resource.Notification.InvalidData });

            // TODO: اگر ResCode == "0" بود باید verify (bpVerifyRequest) و بعد settle انجام شود

            var ok = resCode == "0";

            return Task.FromResult(new GatewayCallbackResultDto
            {
                IsSuccess = ok,
                RefNumber = saleReferenceId,
                Description = $"ResCode={resCode}; SaleOrderId={saleOrderId}"
            });
        }
    }
}
