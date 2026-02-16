using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentGatewaySrv.Dto
{
    public class GatewayStartResultDto
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public bool PaymentIsLink { get; set; }
        public string PaymentUrl { get; set; }
        public string HtmlForm { get; set; }
        public string Token { get; set; } 
        public string GatewayOrderId { get; set; }
    }
}

