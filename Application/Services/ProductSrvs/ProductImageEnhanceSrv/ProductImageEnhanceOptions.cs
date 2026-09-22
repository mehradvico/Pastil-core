using System.Collections.Generic;

namespace Application.Services.ProductSrvs.ProductImageEnhanceSrv
{
    public class ProductImageEnhanceOptions
    {
        public const string SectionName = "ProductImageEnhance";

        public bool Enabled { get; set; } = true;

        // کلید/BaseUrl از همان PastilAI:Providers خوانده می‌شود؛ فقط نام Provider اینجا انتخاب می‌شود.
        public string ProviderName { get; set; } = "Liara";
        public List<string> FallbackProviderNames { get; set; } = new();

        // باید مدلی باشد که هم تصویر می‌گیرد و هم تصویر برمی‌گرداند (نه مدل Vision که فقط متن می‌دهد).
        public string Model { get; set; } = "openai/gpt-image-1-mini";
        public string ImageEditsPath { get; set; } = "images/edits";

        // اپ تا ۹۰ ثانیه منتظر پاسخ می‌ماند؛ تولید تصویر معمولاً ۱۰-۴۰ ثانیه طول می‌کشد.
        public int RequestTimeoutSeconds { get; set; } = 80;

        public long MaxImageSizeBytes { get; set; } = 8 * 1024 * 1024;

        // بوم خروجی مربع است؛ ۱۰۲۴ چون gpt-image-1 همین را تولید می‌کند و تغییر اندازه‌ی دوباره فقط کیفیت را کم می‌کند.
        public int OutputSize { get; set; } = 1024;

        // حاشیه‌ی یکسان دور محصول (درصد از ضلع بوم) — همین است که چند تصویر را «یک‌دست» نشان می‌دهد.
        public double PaddingPercent { get; set; } = 7;
    }
}
