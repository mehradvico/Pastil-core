using System.Text;

namespace Application.Common.Security
{
    /// <summary>
    /// کلید امضای JWT (HS256) باید حداقل ۲۵۶ بیت (۳۲ بایت) و غیرخالی باشد؛ کلید کوتاه با brute-force آفلاین از روی یک توکن قابل کشف است
    /// و کشف آن یعنی جعل توکن ادمین. فقط هشدار می‌دهیم (نه توقف) تا کلید فعلیِ سرور با deploy از کار نیفتد.
    /// </summary>
    public static class JwtKeyPolicy
    {
        public const int RecommendedMinimumBytes = 32;

        public static bool IsWeak(string key) =>
            string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < RecommendedMinimumBytes;
    }
}
