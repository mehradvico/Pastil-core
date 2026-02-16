using Application.Common.Enumerable;
using Application.Services.Order.PaymentGatewaySrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Order.PaymentGatewaySrv
{
    public class PaymentGatewayResolver : IPaymentGatewayResolver
    {
        private readonly IDictionary<MerchantEnum, IPaymentGateway> _map;

        public PaymentGatewayResolver(IEnumerable<IPaymentGateway> gateways)
        {
            _map = gateways.ToDictionary(x => x.Provider, x => x);
        }

        public IPaymentGateway Resolve(MerchantEnum provider)
        {
            if (_map.TryGetValue(provider, out var gateway))
                return gateway;

            throw new InvalidOperationException($"Payment gateway not implemented for provider: {provider}");
        }
    }
}
