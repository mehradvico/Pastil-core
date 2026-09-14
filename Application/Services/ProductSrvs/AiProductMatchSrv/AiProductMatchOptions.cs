namespace Application.Services.ProductSrvs.AiProductMatchSrv
{
    public class AiProductMatchOptions
    {
        public const string SectionName = "AiProductMatch";

        public bool Enabled { get; set; } = true;

        // باید دقیقاً برابر یکی از Name های موجود در PastilAI:Providers باشد (کلید و مدل از همان‌جا خوانده می‌شود)
        public string ProviderName { get; set; } = "Gemini";

        public int MaxImagesPerRequest { get; set; } = 8;
        public int MaxImageSizeBytes { get; set; } = 8 * 1024 * 1024;
        public int CandidateShortlistSize { get; set; } = 25;
        public int RequestTimeoutSeconds { get; set; } = 45;

        // حداقل امتیاز اطمینان برای انتخاب خودکار یک محصول به‌عنوان بهترین تطبیق؛
        // پایین‌تر از این فقط در matches[] پیشنهاد می‌شود، productId اصلی خالی می‌ماند.
        public double AutoSelectConfidenceThreshold { get; set; } = 0.55;
    }
}
