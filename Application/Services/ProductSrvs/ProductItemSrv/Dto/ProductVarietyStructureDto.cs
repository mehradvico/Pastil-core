using System.Collections.Generic;

namespace Application.Services.ProductSrvs.ProductItemSrv.Dto
{
    /// <summary>ساختار تنوع یک محصول (تعیین‌شده توسط ادمین) و مقدارهای قابل‌انتخاب برای فروشنده.</summary>
    public class ProductVarietyStructureDto
    {
        public long ProductId { get; set; }
        /// <summary>null = محصول بدون تنوع (فروشنده فقط یک قیمت/موجودی می‌دهد).</summary>
        public ProductVarietyDto Variety { get; set; }
        public ProductVarietyDto Variety2 { get; set; }
    }

    public class ProductVarietyDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public List<ProductVarietyValueDto> Values { get; set; } = new();
    }

    public class ProductVarietyValueDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
    }
}
