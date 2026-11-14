using Application.Services.ProductSrvs.VarietySrv.Dto;
using System.Collections.Generic;

namespace Application.Services.ProductSrvs.ProductItemSrv.Dto
{
    public class ProductItemForProductDto
    {

        public VarietyShowVDto Variety1 { get; set; }
        public VarietyShowVDto Variety2 { get; set; }
        /// <summary>همه‌ی آیتم‌های قابل‌فروش این کالا از همه‌ی فروشگاه‌ها، به ترتیب قیمت از کم به زیاد.</summary>
        public List<ProductItemForProductVDto> ProductItems { get; set; } = new List<ProductItemForProductVDto>();
        /// <summary>فروشگاه‌های دارای این کالا (هر فروشگاه ارزان‌ترین آیتمش) از کم به زیاد.</summary>
        public List<ProductStoreOfferVDto> Offers { get; set; } = new List<ProductStoreOfferVDto>();
        /// <summary>اگر storeId داده شده و آن فروشگاه این کالا را موجود دارد، همان؛ وگرنه null (پیش‌فرض = ارزان‌ترین).</summary>
        public long? PreferredStoreId { get; set; }
    }
}
