using System.Collections.Generic;

namespace Application.Services.ProductSrvs.ProductItemSrv.Dto
{
    public class ProductItemListRequestDto
    {
        public long StoreId { get; set; }
        public long ProductId { get; set; }
        /// <summary>اختیاری: فقط این مقدارهای تنوع اول (به‌علاوه‌ی آیتم‌های موجود همین فروشگاه). خالی = همه (رفتار قدیمی).</summary>
        public List<long> VarietyItemIds { get; set; }
        /// <summary>اختیاری: مثل بالا برای تنوع دوم.</summary>
        public List<long> Variety2ItemIds { get; set; }


    }
}
