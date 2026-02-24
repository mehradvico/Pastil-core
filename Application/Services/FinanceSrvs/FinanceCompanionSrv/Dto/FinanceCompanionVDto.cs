using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto
{
    public class FinanceCompanionVDto
    {
        public long CompanionId { get; set; }
        public int ReserveCount { get; set; }
        public double TotalCompanionShare { get; set; }
        public double TotalSiteShare { get; set; }
        public List<FinanceCompanionReserveVDto> FinanceCompanionReserves { get; set; }
    }
}
