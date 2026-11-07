using System;

namespace Application.Services.Accounting.UserTokenSrv
{
    // چندنشستیِ نماینده: هر شماره‌ی نماینده (صاحب مرکز/ارائه‌دهنده، کارمند ارائه‌دهنده، پرسنل مرکز — نگاه کنید به
    // UserTokenService.IsPartnerAsync) تا N دستگاه هم‌زمانِ کلاینت (وب‌اپ/اپ) دارد و ورود N+1‌ام قدیمی‌ترین نشست را می‌بندد.
    // N از Security:CompanionOwnerMaxSessions (env PASTIL_COMPANION_OWNER_MAX_SESSIONS) خوانده می‌شود؛ خالی = ۳، 1 = خاموش
    // (هر ورود نشست‌های قبلی هم‌نوع را می‌بندد). نشست پنل ادمین و مسیرهای حساس (تغییر رمز/موبایل، ریست رمز) شامل این استثنا نمی‌شوند.
    public static class SessionLimitRules
    {
        public const string ConfigKey = "Security:CompanionOwnerMaxSessions";
        public const int DefaultMax = 3;
        public const int HardMax = 10;

        // چند نشستِ زنده‌ی قبلی هنگام ورود جدید نگه داشته شود (۰ = همه بسته شوند)
        public static int SessionsToKeepOnSignIn(string configuredMax, bool isPanelLogin, bool isPartner)
        {
            if (isPanelLogin || !isPartner)
                return 0;
            var max = string.IsNullOrWhiteSpace(configuredMax)
                ? DefaultMax
                : int.TryParse(configuredMax, out var parsed) ? parsed : DefaultMax;
            if (max <= 1)
                return 0;
            return Math.Min(max, HardMax) - 1;
        }
    }
}
