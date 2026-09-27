using System;

namespace Application.Services.CompanionSrvs.ConsultationOnlineSrv
{
    // "من آنلاینم": وضعیت دکتر/اپراتور برای مشاوره‌ی آنلاین. روشن‌کردن یک بار برای مدت محدود اعتبار دارد
    // تا اگر فراموش شد خاموش کند، ساعت ۲ شب چراغ سبز کاذب نماند.
    public static class ConsultationOnlineRules
    {
        public static readonly TimeSpan OnlineWindow = TimeSpan.FromHours(3);

        public static DateTime ExpiresAt(DateTime now) => now.Add(OnlineWindow);

        // فقط وقتی واقعاً «روشن» است و هنوز منقضی نشده
        public static bool IsOnline(bool flag, DateTime? expiresAt, DateTime now)
            => flag && expiresAt.HasValue && now < expiresAt.Value;
    }
}
