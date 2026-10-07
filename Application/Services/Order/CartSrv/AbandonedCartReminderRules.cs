using System;

namespace Application.Services.Order.CartSrv
{
    // قواعد خالص یادآوری «سبد خرید رها شده» (پوش ساعت ۱۸ و پیامک ساعت ۲۴). طراحی: backend/Docs/ABANDONED_CART_REMINDER_FA.md
    public static class AbandonedCartReminderRules
    {
        /// <summary>حداکثر تعداد ارسال از هر کانال (پوش/پیامک) برای یک «سبد» تا وقتی کاربر دوباره کالایی اضافه نکرده؛ یعنی فقط ۲ روز.</summary>
        public const int MaxSendsPerChannel = 2;

        /// <summary>سبدی که کمتر از این مدت از آخرین کالای افزوده‌شده‌اش گذشته هنوز «در حال خرید» حساب می‌شود.</summary>
        public static readonly TimeSpan MinIdle = TimeSpan.FromHours(2);

        /// <summary>سقف اطمینان: سبدی که آخرین فعالیتش از این قدیمی‌تر است دیگر یادآوری نمی‌گیرد.</summary>
        public static readonly TimeSpan MaxAge = TimeSpan.FromDays(3);

        public const int ProductNameMaxLength = 30;

        // lastActivity: تاریخ آخرین کالایی که به سبد اضافه شده. sentSinceActivity: تعداد ارسال‌های همین کانال بعد از آن تاریخ.
        public static bool IsEligible(DateTime lastActivity, DateTime now, int sentSinceActivity)
        {
            var age = now - lastActivity;
            return age >= MinIdle && age <= MaxAge && sentSinceActivity < MaxSendsPerChannel;
        }

        // نام کالا در متن پوش/پیامک: فاصله‌های اضافه حذف و از آخرین فاصله‌ی قبل از سقف طول بریده می‌شود تا وسط کلمه قطع نشود.
        public static string ShortProductName(string name)
        {
            var clean = string.Join(' ', (name ?? string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
            if (clean.Length <= ProductNameMaxLength)
                return clean;
            var cut = clean.Substring(0, ProductNameMaxLength);
            var lastSpace = cut.LastIndexOf(' ');
            return (lastSpace > 10 ? cut.Substring(0, lastSpace) : cut).TrimEnd() + "…";
        }

        // فقط نام کوچک (اولین کلمه)؛ null یعنی «نام پیش‌فرض» جایگزین شود.
        public static string FirstNameOnly(string firstName)
        {
            var parts = (firstName ?? string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : null;
        }
    }
}
