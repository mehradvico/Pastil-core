using System;

namespace Application.Services.ProviderSrvs.ProviderSubscriptionSrv
{
    /// <summary>قواعد خالص اشتراک (بدون وابستگی به DB) تا سرویس و نگاشت DTO از یک تعریف استفاده کنند.</summary>
    public static class ProviderSubscriptionRules
    {
        public static bool IsActiveAt(DateTime startDate, DateTime endDate, DateTime now)
            => startDate <= now && endDate > now;

        public static bool IsValidPeriod(DateTime startDate, DateTime endDate)
            => endDate > startDate;

        public const int SilverLevel = 1;
        public const int GoldLevel = 2;

        /// <summary>
        /// نگاشت سطح اشتراک به فیلدهای قدیمیِ Companion (Gold/SilverAccountDate) که هنوز جستجو، مرتب‌سازی و badge از آن‌ها ساخته می‌شود.
        /// سطح >= طلایی (طلایی و الماس) → Gold؛ سطح نقره‌ای → Silver؛ بدون اشتراک → هر دو null.
        /// </summary>
        public static (DateTime? Gold, DateTime? Silver, DateTime? SilverCreate) ToLegacyAccountDates(int? level, DateTime startDate, DateTime endDate)
        {
            if (!level.HasValue || level.Value < SilverLevel)
                return (null, null, null);
            if (level.Value >= GoldLevel)
                return (endDate, null, null);
            return (null, endDate, startDate);
        }
    }
}
