using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentGatewaySrv.Dto
{
    public class GatewayCallbackResultDto
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string RefNumber { get; set; }
        public string TraceNumber { get; set; }
        public string Description { get; set; }
        public string Token { get; set; }
    }
}


