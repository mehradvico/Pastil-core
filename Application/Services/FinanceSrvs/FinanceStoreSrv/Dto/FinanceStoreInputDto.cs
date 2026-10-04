using Application.Services.FinanceSrvs.FinanceStoreSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceStoreSrv.Dto
{
    public class FinanceStoreInputDto : IFinanceStoreSearchFields
    {
        public long StoreId { get; set; }
        public bool? Permitted { get; set; }
        // فیلتر تحویل‌گیری مشتری: 0 = منتظر پاسخ (ارسال‌شده و بی‌پاسخ)، 1 = تحویل گرفته، 2 = تحویل نگرفته
        public int? UserDelivery { get; set; }
    }
}
