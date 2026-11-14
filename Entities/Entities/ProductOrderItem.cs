using Entities.Entities.CommonField;

namespace Entities.Entities
{
    public class ProductOrderItem : Id_Field
    {
        public long ProductItemId { get; set; }
        public int Count { get; set; }
        public double Price { get; set; }
        public double BasePrice { get; set; }
        public double DiscountPrice { get; set; }
        public int DiscountPercent { get; set; }
        /// <summary>مبلغ تخفیفِ سهم پاستیل به‌ازای «یک واحد» کالا هنگام ثبت سفارش (BasePrice × PastilDiscountPercent ÷ ۱۰۰). حسابداری: سهم فروشگاه روی (پرداختی + این مبلغ) حساب می‌شود.</summary>
        public double PastilDiscountUnit { get; set; }
        public long ProductOrderStoreId { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }
        public bool Edited { get; set; }
        public ProductItem ProductItem { get; set; }
        public ProductOrderStore ProductOrderStore { get; set; }
    }
}
