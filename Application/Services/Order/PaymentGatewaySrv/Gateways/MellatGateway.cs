using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.Order.PaymentGatewaySrv.Dto;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentGatewaySrv.Gateways
{
    public class MellatGateway : IPaymentGateway
    {
        public MerchantEnum Provider => MerchantEnum.mellat;

        public Task<GatewayStartResultDto> StartAsync(PaymentStartDto dto, Merchant merchant)
        {
            return Task.FromResult(new GatewayStartResultDto
            {
                IsSuccess = false,
                ErrorMessage = "Mellat gateway is not configured for production mode."
            });
        }

        public Task<GatewayCallbackResultDto> CallbackAsync(Payment payment, Merchant merchant, HttpRequest request)
        {
            return Task.FromResult(new GatewayCallbackResultDto
            {
                IsSuccess = false,
                ErrorMessage = "Mellat gateway is not configured for production mode."
            });
        }
    }
}
