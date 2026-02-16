using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentGatewaySrv.Dto
{
    public class PaymentGatewayLogVDto
    {
        public long PaymentId { get; set; }
        public string Provider { get; set; }
        public bool Success { get; set; }
        public string Token { get; set; }
        public string RefNumber { get; set; }
        public string TraceNumber { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
