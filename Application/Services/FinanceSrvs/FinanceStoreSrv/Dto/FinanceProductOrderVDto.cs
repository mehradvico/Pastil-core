using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceStoreSrv.Dto
{
    public class FinanceProductOrderVDto
    {
        public string ProductOrderId { get; set; }
        public string OrderCode { get; set; }
        public string BuyerFullName { get; set; }
        public double PaymentPrice { get; set; }
        public decimal CommissionPercent { get; set; }
        public double StoreShare { get; set; }
        public double SiteShare { get; set; }
        public string StatusLabel { get; set; }
        public bool Permitted { get; set; }
    }
}
