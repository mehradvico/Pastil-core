using System;

namespace Application.Services.Accounting.UserTokenSrv
{
    // پل موقت: صاحب مرکز (نماینده) فعلاً حسابش را بین کارمندانش پخش می‌کند، چون حساب کارمند (CompanionUser) هنوز در بیشتر
    // endpointهای ناحیه‌ی Companion کار نمی‌کند. با Security:CompanionOwnerMaxSessions (env PASTIL_COMPANION_OWNER_MAX_SESSIONS)
    // صاحب مرکزِ فعال و تأییدشده تا N نشست هم‌زمانِ کلاینت (وب‌اپ/اپ) دارد و ورود N+1‌ام قدیمی‌ترین را می‌بندد.
    // 1 یا خالی = خاموش (رفتار قبلی: هر ورود نشست‌های قبلی هم‌نوع را می‌بندد). نشست پنل ادمین و مسیرهای حساس
    // (تغییر رمز/موبایل، ریست رمز) شامل این استثنا نمی‌شوند. بعد از راه افتادن حساب کارمندان خاموش شود.
    public static class SessionLimitRules
    {
        public const string ConfigKey = "Security:CompanionOwnerMaxSessions";
        public const int HardMax = 10;

        // چند نشستِ زنده‌ی قبلی هنگام ورود جدید نگه داشته شود (۰ = همه بسته شوند)
        public static int SessionsToKeepOnSignIn(string configuredMax, bool isPanelLogin, bool isCompanionOwner)
        {
            if (isPanelLogin || !isCompanionOwner)
                return 0;
            if (!int.TryParse(configuredMax, out var max) || max <= 1)
                return 0;
            return Math.Min(max, HardMax) - 1;
        }
    }
}
