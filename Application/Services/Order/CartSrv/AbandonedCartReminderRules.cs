using System;

namespace Application.Services.Order.CartSrv
{
    // قواعد خالص یادآوری «سبد خرید رها شده»، نسبی به آخرین کالای افزوده‌شده (تأیید ۱۴۰۵/۰۸/۲۳):
    //   پوش اول ۱ ساعت بعد، پوش دوم ۳ ساعت بعد (و دست‌کم ۲ ساعت بعد از پوش اول)، پیامک ۲۴ ساعت بعد. بدون تخفیف.
    // هر کالای تازه در سبد شمارش را از نو شروع می‌کند؛ سبد خالی (سفارش یا حذف) هیچ یادآوری نمی‌گیرد.
    // ساعت سکوت در MarketingQuietHours است. طراحی: backend/Docs/ABANDONED_CART_REMINDER_FA.md
    public static class AbandonedCartReminderRules
    {
        public const int MaxPushes = 2;
        public const int MaxSms = 1;

        public static readonly TimeSpan FirstPushIdle = TimeSpan.FromHours(1);
        public static readonly TimeSpan SecondPushIdle = TimeSpan.FromHours(3);
        public static readonly TimeSpan MinGapBetweenPushes = TimeSpan.FromHours(2);
        /// <summary>بعد از این مدت پوش دیگری نمی‌رود؛ نوبت پیامک است.</summary>
        public static readonly TimeSpan PushMaxAge = TimeSpan.FromHours(24);
        public static readonly TimeSpan SmsIdle = TimeSpan.FromHours(24);

        /// <summary>سقف اطمینان: سبدی که آخرین فعالیتش از این قدیمی‌تر است دیگر یادآوری نمی‌گیرد.</summary>
        public static readonly TimeSpan MaxAge = TimeSpan.FromDays(3);

        /// <summary>سبدی که کمتر از این مدت از آخرین کالایش گذشته اصلاً نامزد یادآوری نیست.</summary>
        public static TimeSpan MinIdle => FirstPushIdle;

        public const int ProductNameMaxLength = 30;

        // شماره‌ی پوشی که الان باید برود (۱ یا ۲) یا ۰. pushesSent/lastPushAt: پوش‌های سبد رها شده بعد از آخرین فعالیت.
        public static int NextPush(DateTime lastActivity, DateTime now, int pushesSent, DateTime? lastPushAt)
        {
            var age = now - lastActivity;
            if (age > PushMaxAge || pushesSent >= MaxPushes)
                return 0;
            if (pushesSent == 0)
                return age >= FirstPushIdle ? 1 : 0;
            var gapOk = !lastPushAt.HasValue || now - lastPushAt.Value >= MinGapBetweenPushes;
            return age >= SecondPushIdle && gapOk ? 2 : 0;
        }

        public static bool IsSmsDue(DateTime lastActivity, DateTime now, int smsSent)
        {
            var age = now - lastActivity;
            return age >= SmsIdle && age <= MaxAge && smsSent < MaxSms;
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
