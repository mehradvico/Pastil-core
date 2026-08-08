using Application.Services.Order.PaymentGatewaySrv.Dto;
using Application.Services.Order.PaymentSrv.Dto;
using Entities.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Order.PaymentGatewaySrv.Iface
{
    public interface IPaymentTestModeService
    {
        bool IsEnabled { get; }

        void ConfigureStartResult(PaymentStartDto dto);

        GatewayCallbackResultDto CreateCallbackResult(Payment payment, HttpRequest request);
    }
}
