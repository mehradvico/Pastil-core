using Application.Common.Enumerable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentGatewaySrv.Iface
{
    public interface IPaymentGatewayResolver
    {
        IPaymentGateway Resolve(MerchantEnum provider);
    }
}
