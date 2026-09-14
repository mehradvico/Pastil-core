namespace Application.Services.ProductSrvs.AiProductMatchSrv.Dto
{
    // یک ترکیب تنوع معتبر برای محصول (طبق تصمیم کل‌کاتالوگ، نه فقط موجودی فروشگاه فعلی)
    public class AiProductMatchPackageDto
    {
        // null یعنی این ترکیب تنوع هنوز برای فروشگاه جاری به‌عنوان ProductItem ساخته نشده
        public long? ProductItemId { get; set; }
        public string Label { get; set; }
        public long? VarietyItemId { get; set; }
        public long? VarietyItem2Id { get; set; }
        public bool ExistsForCurrentStore { get; set; }
    }
}
