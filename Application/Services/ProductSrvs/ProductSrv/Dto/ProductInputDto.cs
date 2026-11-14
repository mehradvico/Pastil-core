

using Application.Common.Dto.Input;
using Application.Common.Dto.LocationPoint;
using Application.Common.Enumerable;
using Application.Services.ProductSrv.Iface;

namespace Application.Services.ProductSrv.Dto
{
    public class ProductInputDto : BaseInputDto, IProductSearchFields
    {
        public bool? Active { get; set; }
        public long[] CategoryIds { get; set; }
        public string[] CategoryLabels { get; set; }
        public bool IsAndCategories { get; set; }
        public long? NotId { get; set; }
        public double? PriceFrom { get; set; }
        public double? PriceTo { get; set; }
        public bool? InStock { get; set; }
        public long[] BrandIds { get; set; }
        public long[] FeatureItemIds { get; set; }
        public long? StoreId { get; set; }
        /// <summary>فقط endpoint عمومی (صفحه‌ی فروشگاه) true می‌کند: قیمت/مرتب‌سازی از آیتم‌های همان فروشگاه. پنل فروشنده/ادمین false می‌مانند.</summary>
        public bool ApplyStorePrice { get; set; }
        public long? CreateStoreId { get; set; }

        public bool? HasDiscount { get; set; }
        public string DiscountGroupLabel { get; set; }
        public bool IsAdmin { get; set; }

        public ProductStatusEnum? Status { get; set; }
        public ProductTypeEnum? Type { get; set; }
        public PointDto Distance { get; set; }
    }
}
