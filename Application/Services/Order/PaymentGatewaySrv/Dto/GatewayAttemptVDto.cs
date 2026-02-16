using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PaymentGatewaySrv.Dto
{
    public class GatewayAttemptVDto
    {
        public long PaymentId { get; set; }
        public string Provider { get; set; }
        public string Token { get; set; }
        public string RefNumber { get; set; }
        public bool Success { get; set; }
        public DateTime CreateDate { get; set; }
        public string Description { get; set; }
    }
}
