using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.FinanceSrv.Iface
{
    public interface IFinanceSearchFields
    {
        public bool? IsCompanion { get; set; }
        public bool? HasCommission { get; set; }
    }
}
