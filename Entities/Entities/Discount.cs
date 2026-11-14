using Entities.Entities.CommonField;
using System;

namespace Entities.Entities
{
    public class Discount : Id_Field
    {
        public long TypeId { get; set; }
        public long DiscountGroupId { get; set; }
        public DateTime? EndDate { get; set; }
        public long StoreId { get; set; }
        public int Percent { get; set; }
        public bool Synced { get; set; }
        public long? CategoryId { get; set; }
        public long? BrandId { get; set; }
        public long? ProductId { get; set; }
        public long? ProductItemId { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        /// <summary>true = تخفیفِ «سهم پاستیل» (فقط ادمین می‌سازد؛ درصدش ≤ سهم پاستیل از فروش این فروشگاه؛ هزینه‌اش با پاستیل است). false = تخفیف خودِ فروشگاه (هزینه‌اش با فروشگاه).</summary>
        public bool FundedByPastil { get; set; }

        public DiscountGroup DiscountGroup { get; set; }
        public Code Type { get; set; }
        public Category Category { get; set; }
        public Brand Brand { get; set; }
        public Product Product { get; set; }
        public ProductItem ProductItem { get; set; }
        public Store Store { get; set; }

    }
}
