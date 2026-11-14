using Application.Services.ProductSrvs.StoreSrv.Dto;

namespace Application.Services.ProductSrvs.ProductItemSrv.Dto
{
    /// <summary>
    /// پیشنهاد یک فروشگاه برای این کالا (ارزان‌ترین آیتمِ موجودِ آن فروشگاه). لیست «فروشگاه‌هایی که این کالا را دارند»
    /// همیشه به ترتیب قیمت از کم به زیاد برمی‌گردد.
    /// </summary>
    public class ProductStoreOfferVDto
    {
        public long StoreId { get; set; }
        public StoreMinVDto Store { get; set; }
        /// <summary>آیتمِ ارزان‌ترین پیشنهاد این فروشگاه (برای افزودن به سبد).</summary>
        public long ProductItemId { get; set; }
        public long Price { get; set; }
        public long BasePrice { get; set; }
        public int DiscountPercent { get; set; }
        public int Quantity { get; set; }
        /// <summary>true = فروشگاهی که کاربر از صفحه‌ی آن آمده (پارامتر storeId) و باید پیش‌فرض انتخاب شود.</summary>
        public bool IsPreferred { get; set; }
    }
}
