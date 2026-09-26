using System;
using System.Collections.Concurrent;

namespace Application.Services.PastilAISrv.Provider
{
    // وقتی یک ارائه‌دهنده‌ی هوش مصنوعی «ثابت» خراب است (اعتبار تمام‌شده ۴۰۲/۴۲۹، کلید نامعتبر/ممنوع ۴۰۱/۴۰۳، مدل ناموجود ۴۰۴،
    // یا timeout)، هر پیام کاربر مجبور می‌شد تک‌تک آن‌ها را امتحان کند (فقط timeout جمینای ۴۵ ثانیه) و اپ قبل از رسیدن
    // پاسخ از ارائه‌دهنده‌ی سالم خطا می‌داد. این کلاس ارائه‌دهنده‌ی خراب را مدتی کنار می‌گذارد و بعد دوباره امتحان می‌کند.
    public static class PastilAiProviderCooldown
    {
        private static readonly ConcurrentDictionary<string, DateTime> Until = new(StringComparer.OrdinalIgnoreCase);

        // مدت کنار گذاشتن بر اساس نوع خطا؛ null = خطای گذرا (مثلاً ۵۰۰) که کنار گذاشتن ندارد
        public static TimeSpan? Compute(int? httpStatus, string errorCode)
        {
            if (httpStatus is 401 or 402 or 403 or 404)
                return TimeSpan.FromMinutes(15);
            if (httpStatus == 429)
                return TimeSpan.FromMinutes(5);
            if (string.Equals(errorCode, "timeout", StringComparison.OrdinalIgnoreCase))
                return TimeSpan.FromMinutes(2);
            // خطاهای شبکه/DNS نیز تا چند دقیقه پایدارند. تکرار فوری آن‌ها فقط زمان پاسخ
            // را بیشتر می‌کند؛ بنابراین تا بازیابی DNS یا شبکه، این provider را رد کن.
            if (string.Equals(errorCode, "provider_exception", StringComparison.OrdinalIgnoreCase))
                return TimeSpan.FromMinutes(5);
            return null;
        }

        public static void MarkFailed(string provider, int? httpStatus, string errorCode)
        {
            var span = Compute(httpStatus, errorCode);
            if (span.HasValue && !string.IsNullOrWhiteSpace(provider))
                Until[provider] = DateTime.UtcNow + span.Value;
        }

        public static void MarkSucceeded(string provider)
        {
            if (!string.IsNullOrWhiteSpace(provider))
                Until.TryRemove(provider, out _);
        }

        // فقط برای تست‌ها: حالت استاتیک بین تست‌ها نشت نکند
        public static void Clear() => Until.Clear();

        public static bool IsCoolingDown(string provider) =>
            !string.IsNullOrWhiteSpace(provider) && Until.TryGetValue(provider, out var until) && until > DateTime.UtcNow;
    }
}
