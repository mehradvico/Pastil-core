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

        // برای veterinary/sepidar وقتی جدول نرم‌افزار انبار به‌صورت متن قابل‌خواندن نبود و Bridge
        // به‌جای آن، صفحه‌به‌صفحه از جدول اسکرین‌شات می‌گیرد (تا ۴۰ صفحه، نه ۸ مثل عکس قفسه).
        public int MaxTableImagesPerRequest { get; set; } = 40;

        // چند اسکرین‌شات جدول با هم در یک فراخوانی مدل فرستاده شوند، نه یکی‌یکی مثل قفسه — وگرنه
        // ۴۰ تصویر یعنی ۴۰ فراخوانی موازی جدا به GapGPT/Gemini که ریسک Rate-Limit و هزینه‌ی تکراری
        // system prompt را چند برابر می‌کند. عدد کم نگه داشته می‌شود چون هر اسکرین‌شات جدول معمولاً
        // چند ده ردیف دارد و فشردن تصاویر بیشتر در یک تماس، دقت خواندن مدل را کم می‌کند.
        public int TableImagesPerVisionCall { get; set; } = 4;

        public int CandidateShortlistSize { get; set; } = 25;

        // مرحله‌ی تطبیق ردیف‌ها را دسته‌دسته (موازی) به مدل می‌دهد، نه همه در یک پیام: با ۶+ ردیف × ده‌ها کاندید،
        // یک پیام غول‌آسا از Timeout می‌گذشت و «همه‌ی ردیف‌ها» با هم ناموفق می‌شدند.
        public int MatchRowsPerCall { get; set; } = 4;
        public int RequestTimeoutSeconds { get; set; } = 15;

        // سقف سخت کل تحلیل (استخراج + جست‌وجوی کاتالوگ + تطبیق). اپ سقف HTTP حدود ۶۰ ثانیه دارد؛ اگر بودجه
        // تمام شود، سرور به‌جای Timeout نتیجهٔ ناقص برمی‌گرداند (آیتم‌های تطبیق‌نشده با CatalogMatchFailed=true).
        public int TotalBudgetSeconds { get; set; } = 45;

        // بخشی از بودجه که استخراج از عکس اجازهٔ مصرفش را ندارد تا مرحلهٔ تطبیق هم جا بماند.
        public int MatchStageReserveSeconds { get; set; } = 15;

        // حداقل امتیاز اطمینان برای انتخاب خودکار یک محصول به‌عنوان بهترین تطبیق؛
        // پایین‌تر از این فقط در matches[] پیشنهاد می‌شود، productId اصلی خالی می‌ماند.
        public double AutoSelectConfidenceThreshold { get; set; } = 0.80;
    }
}
