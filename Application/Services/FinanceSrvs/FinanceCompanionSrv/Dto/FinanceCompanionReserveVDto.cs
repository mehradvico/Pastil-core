using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto
{
    public class FinanceCompanionReserveVDto
    {
        public string ReserveId { get; set; }
        public string BookerFullName { get; set; }
        public double PaymentPrice { get; set; }
        public decimal CommissionPercent { get; set; }
        public double CompanionShare { get; set; }
        public double SiteShare { get; set; }
        public string StatusLabel { get; set; }
        public bool IsPansion { get; set; }
        public bool Permitted { get; set; }
    }
}
