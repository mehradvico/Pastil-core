using System;

namespace Application.Services.MarketingSrv
{
    // ساعت سکوت پیام‌های یادآوری/بازاریابی (سند اتوماسیون پیام، تأیید ۱۴۰۵/۰۸/۲۳). همه به وقت تهران:
    //   پوش: نه بین ۲۱:۳۰ تا ۰۹:۰۰.
    //   پیامک: نه بین ۲۱:۰۰ تا ۰۹:۰۰ و نه جمعه پیش از ۱۱:۰۰.
    //   پیامک سبد رها شده: فقط ۱۰:۰۰ تا ۱۹:۰۰ (جمعه از ۱۱:۰۰).
    // پیامی که در ساعت سکوت بیفتد حذف نمی‌شود؛ jobها هر چند دقیقه اجرا می‌شوند و اولین اجرای بعد از پایان سکوت آن را می‌فرستند.
    // پیام‌های تراکنشی (کد ورود، تأیید پرداخت، راننده در راه، ...) از این قاعده رد نمی‌شوند.
    public static class MarketingQuietHours
    {
        private static readonly TimeZoneInfo TehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

        public static DateTime TehranNow() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TehranTimeZone);

        public static bool IsPushAllowed(DateTime tehranTime)
        {
            var t = tehranTime.TimeOfDay;
            return t >= new TimeSpan(9, 0, 0) && t < new TimeSpan(21, 30, 0);
        }

        public static bool IsSmsAllowed(DateTime tehranTime) => IsInsideSmsWindow(tehranTime, new TimeSpan(9, 0, 0), new TimeSpan(21, 0, 0));

        public static bool IsCartSmsAllowed(DateTime tehranTime) => IsInsideSmsWindow(tehranTime, new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0));

        private static bool IsInsideSmsWindow(DateTime tehranTime, TimeSpan start, TimeSpan end)
        {
            var t = tehranTime.TimeOfDay;
            if (tehranTime.DayOfWeek == DayOfWeek.Friday && start < new TimeSpan(11, 0, 0))
                start = new TimeSpan(11, 0, 0);
            return t >= start && t < end;
        }
    }
}
