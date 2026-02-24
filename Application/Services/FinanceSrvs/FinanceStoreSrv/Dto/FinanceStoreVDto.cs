using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceStoreSrv.Dto
{
    public class FinanceStoreVDto
    {
        public long StoreId { get; set; }
        public int ProductOrderCount { get; set; }
        public double TotalStoreShare { get; set; }
        public double TotalSiteShare { get; set; }
        public List<FinanceProductOrderVDto> FinanceProductOrders { get; set; }
    }
}
