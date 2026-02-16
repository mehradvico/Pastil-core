using Application.Common.Enumerable;
using Application.Services.Order.PaymentGatewaySrv.Dto;
using Application.Services.Order.ProductOrderSrv.Dto;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentGatewaySrv.Iface
{
    public interface IPaymentGateway
    {
        MerchantEnum Provider { get; }
        Task<GatewayStartResultDto> StartAsync(PaymentStartDto dto, Merchant merchant);
        Task<GatewayCallbackResultDto> CallbackAsync(Payment payment, Merchant merchant, HttpRequest request, bool testMode);
    }
}
