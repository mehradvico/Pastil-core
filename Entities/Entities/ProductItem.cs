using Entities.Entities.CommonField;
using System;

namespace Entities.Entities
{
    public class ProductItem : Id_Field
    {
        public long BasePrice { get; set; }
        public long Price { get; set; }
        public int DiscountPercent { get; set; }
        /// <summary>بخشی از DiscountPercent که سهم پاستیل است (procedure UpdateProductItemDiscount پر می‌کند). درصدِ تخفیفِ پاستیل است که روی «قیمتِ بعد از تخفیف فروشگاه» اعمال می‌شود (DiscountPercent = جمع مؤثر هر دو لایه، گردشده).</summary>
        public int PastilDiscountPercent { get; set; }
        /// <summary>تخفیفِ سهم پاستیل به‌ازای یک واحد (تومان) = قیمتِ بعد از تخفیف فروشگاه − قیمت نهایی. همین مبلغ هنگام سفارش در ProductOrderItem.PastilDiscountUnit ثبت می‌شود.</summary>
        public double PastilDiscountAmount { get; set; }
        public long? DiscountGroupId { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public long? VarietyItemId { get; set; }
        public long? VarietyItem2Id { get; set; }
        public long ProductId { get; set; }
        public long StoreId { get; set; }
        public int Quantity { get; set; }
        public bool Active { get; set; }
        public string Warranty { get; set; }
        public bool SystemActive { get; set; }
        public bool Deleted { get; set; }
        public Product Product { get; set; }
        public VarietyItem VarietyItem { get; set; }
        public VarietyItem VarietyItem2 { get; set; }
        public Store Store { get; set; }
        public DiscountGroup DiscountGroup { get; set; }

    }
}
