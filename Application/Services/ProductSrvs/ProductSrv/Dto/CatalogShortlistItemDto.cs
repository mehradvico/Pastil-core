namespace Application.Services.ProductSrvs.ProductSrv.Dto
{
    // یک محصول کاتالوگ در فهرست کوتاه جست‌وجوی دسته‌ای (AiProductMatch). فقط سه فیلدی که برای
    // نمره‌دادن شباهت نام در حافظه لازم است؛ بقیهٔ اطلاعات بعداً فقط برای همین شناسه‌ها لود می‌شود.
    public class CatalogShortlistItemDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string BrandName { get; set; }
    }
}
