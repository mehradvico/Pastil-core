using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceStoreSrv.Iface
{
    public interface IFinanceStoreSearchFields
    {
        public bool? Paid { get; set; }
    }
}
