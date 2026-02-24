using Application.Services.FinanceSrvs.FinanceCompanionSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto
{
    public class FinanceCompanionInputDto : IFinanceCompanionSearchFields
    {
        public long CompanionId { get; set; }
        public bool? Permitted { get; set; }
    }
}
