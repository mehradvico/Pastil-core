using System;

namespace Application.Services.TripSrv.TripSrv
{
    // قواعد خالص ساخت نوبت‌های سرویس هفتگی پت‌رسان (بدون دیتابیس) تا تست‌پذیر باشد.
    public static class PetResanServiceTripRules
    {
        /// <summary>ساعت اجرای job روزانه‌ی ساخت نوبت‌های «فردا» (به وقت تهران؛ باید با Program.cs یکی باشد: "0 20 * * *").</summary>
        public const int DailyJobHourTehran = 20;

        /// <summary>
        /// job امروز برای «فردا» اجرا شده است؟ اگر بله، سرویسی که بعد از آن ثبت می‌شود نوبت فردایش را تا job فردا
        /// (که فقط «پس‌فردا» را می‌سازد) نمی‌گیرد؛ پس باید همان لحظه‌ی ثبت ساخته شود.
        /// </summary>
        public static bool DailyJobHasRun(DateTime tehranNow) => tehranNow.Hour >= DailyJobHourTehran;

        public static DateTime TehranNow(DateTime utcNow)
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), zone);
        }
    }
}
