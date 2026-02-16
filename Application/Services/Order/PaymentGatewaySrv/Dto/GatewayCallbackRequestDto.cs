using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentGatewaySrv.Dto
{
    public class GatewayCallbackRequestDto
    {
        public IDictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
    }
}

