using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.TripSrv.TripSrv
{
    // قواعد خالص ساخت نوبت‌های سرویس هفتگی پت‌رسان (بدون دیتابیس) تا تست‌پذیر باشد.
    public static class PetResanServiceTripRules
    {
        /// <summary>چند روزِ سرویسِ پشت‌سرهم بدون راننده (لغو خودکار) تا سرویس هفتگی خودکار متوقف شود.</summary>
        public const int StopAfterNoDriverDays = 2;

        /// <summary>
        /// نوبت‌های تمام‌شده‌ی (تکمیل/لغو) یک سرویس: آخرین <paramref name="days"/> روزِ سرویس همه «بدون راننده» بوده‌اند؟
        /// شمارش بر اساس روز است نه سفر، چون سرویس با ساعت برگشت در هر روز دو سفر جدا (رفت/برگشت) دارد و لغو هر دو در یک روز
        /// نباید یعنی «دو بار». روزی «بدون راننده» است که همه‌ی سفرهای تمام‌شده‌اش لغو خودکار بی‌راننده باشند؛ یک سفر تکمیل‌شده
        /// یا لغو کاربر/ادمین/راننده در آن روز زنجیره را می‌شکند.
        /// </summary>
        public static bool ShouldStopForNoDriver(IEnumerable<(DateTime Start, bool NoDriverAutoCancel)> finishedOccurrences, int days = StopAfterNoDriverDays)
        {
            var lastDays = finishedOccurrences
                .GroupBy(o => o.Start.Date)
                .OrderByDescending(g => g.Key)
                .Take(days)
                .ToList();
            return lastDays.Count == days && lastDays.All(day => day.All(o => o.NoDriverAutoCancel));
        }

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
