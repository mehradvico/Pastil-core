using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.FinanceSrv.Dto
{
    public class FinanceStoreDto
    {
        public long StoreId { get; set; }
        public decimal CommissionPercent { get; set; }
    }
}
