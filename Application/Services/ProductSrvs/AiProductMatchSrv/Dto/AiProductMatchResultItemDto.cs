using System.Collections.Generic;

namespace Application.Services.ProductSrvs.AiProductMatchSrv.Dto
{
    public class AiProductMatchResultItemDto
    {
        public string RowId { get; set; }
        public string DetectedName { get; set; }
        public string ExternalCode { get; set; }
        public double? Price { get; set; }
        public int? Quantity { get; set; }
        public long? ProductId { get; set; }
        public long? ProductItemId { get; set; }
        public string ProductName { get; set; }
        public double Confidence { get; set; }

        // برند/سایز خوانده‌شده از روی بسته (فقط برای عکس؛ وگرنه null) — اپ برای ساخت «محصول ثبت‌نشده» استفاده می‌کند.
        public string Brand { get; set; }
        public string PackageSize { get; set; }

        // کادر هر بستهٔ قابل‌رؤیت (شلف). خالی = مدل کادر قابل‌اعتماد نداده؛ کادر اشتباه بدتر از بدون کادر است.
        public List<AiProductMatchBoundingBoxDto> BoundingBoxes { get; set; } = new();

        // true یعنی مرحلهٔ تطبیق کاتالوگ برای این آیتم کامل نشد (خطا/تمام‌شدن بودجهٔ زمانی) — نه این‌که واقعاً
        // در کاتالوگ نباشد. اپ نباید چنین آیتمی را «ناموجود» گزارش کند.
        public bool CatalogMatchFailed { get; set; }

        public List<string> Issues { get; set; } = new();
        public List<AiProductMatchCandidateDto> Matches { get; set; } = new();
    }
}
