using System.Collections.Generic;

namespace Application.Services.ProductSrvs.AiProductMatchSrv
{
    public class AiProductMatchOptions
    {
        public const string SectionName = "AiProductMatch";

        public bool Enabled { get; set; } = true;

        // باید دقیقاً برابر یکی از Name های موجود در PastilAI:Providers باشد (کلید و مدل از همان‌جا خوانده می‌شود).
        // Gemini مستقیم از این سرور/شبکه معمولاً یا اصلاً وصل نمی‌شود یا (بدتر) تا مرز Timeout آویزان
        // می‌ماند بدون رد فوری اتصال؛ GapGPT (Gateway ایرانی) به‌عنوان Provider اصلی این فیچر انتخاب شده.
        public string ProviderName { get; set; } = "GapGPT";

        // اگر Provider اصلی پاسخ نداد، فقط همین‌ها (به همین ترتیب) به‌عنوان Fallback امتحان می‌شوند —
        // نه کل فهرست PastilAI:Providers. آن فهرست شامل Providerهایی است که برای چت انتخاب/تنظیم شده‌اند
        // (بعضی موقتاً Rate-Limit یا بدرستی پیکربندی نشده‌اند)؛ امتحان همه‌شان به‌ترتیب، این فیچر را کند و
        // در معرض Timeout سمت کلاینت (بعد از چند Fallback ناموفق پشت‌سرهم) می‌کند. Gemini عمداً این‌جا
        // نیست — چون گاهی به‌جای رد سریع اتصال، تا Timeout آویزان می‌ماند و کل بودجه‌ی زمانی را می‌بلعد.
        public List<string> FallbackProviderNames { get; set; } = new() { "AvalAI" };

        public int MaxImagesPerRequest { get; set; } = 8;
        public int MaxImageSizeBytes { get; set; } = 8 * 1024 * 1024;
        public int CandidateShortlistSize { get; set; } = 25;
        public int RequestTimeoutSeconds { get; set; } = 15;

        // حداقل امتیاز اطمینان برای انتخاب خودکار یک محصول به‌عنوان بهترین تطبیق؛
        // پایین‌تر از این فقط در matches[] پیشنهاد می‌شود، productId اصلی خالی می‌ماند.
        public double AutoSelectConfidenceThreshold { get; set; } = 0.80;
    }
}
