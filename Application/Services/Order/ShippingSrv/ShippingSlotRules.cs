using System;

namespace Application.Services.Order.ShippingSrv
{
    // قواعد خالص بازه‌های تحویل (به وقت تهران). طراحی: backend/Docs/MIARE_SHIPPING_SLOTS_FA.md
    public static class ShippingSlotRules
    {
        private static readonly TimeZoneInfo Tehran = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

        public static DateTime ToTehran(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Tehran);

        // تاریخ (بدون ساعت) + ساعت تهران ← UTC
        public static DateTime ToUtc(DateTime tehranDate, TimeSpan time)
        {
            var local = DateTime.SpecifyKind(tehranDate.Date + time, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(local, Tehran);
        }

        // تحویل «همان روز» نداریم: فقط از فردا تا N روز بعد از آن (به وقت تهران) قابل انتخاب است.
        public static bool IsDateInWindow(DateTime date, DateTime nowUtc, int horizonDays)
        {
            var today = ToTehran(nowUtc).Date;
            var offset = (date.Date - today).Days;
            return offset >= 1 && offset <= Math.Max(1, horizonDays);
        }

        // بازه فقط وقتی برای یک فروشگاه قابل انتخاب است که کالا بعد از «الان + زمان آماده‌سازی فروشگاه + فاصله‌ی اطمینان» هنوز بتواند
        // قبل از «پایان بازه − حداقل زمان رساندن» به پیک تحویل شود؛ وگرنه پیک نمی‌تواند داخل همان بازه تحویل بدهد.
        public static bool IsFeasible(DateTime date, TimeSpan endTime, DateTime nowUtc, int prepMinutes, int bufferMinutes, int minDeliveryMinutes)
        {
            var readyAt = nowUtc.AddMinutes(Math.Max(0, prepMinutes) + Math.Max(0, bufferMinutes));
            return LatestPickup(ToUtc(date, endTime), minDeliveryMinutes) >= readyAt;
        }

        // افق انتخاب: روزهای پایه + روزهایی که آماده‌سازی فروشگاه (مثلاً ۱ روز) عملاً می‌خورد
        public static int HorizonDays(int baseDays, int prepMinutes) =>
            Math.Max(1, baseDays) + (int)Math.Ceiling(Math.Max(0, prepMinutes) / 1440.0);

        // آخرین ساعتی که فروشنده باید «آماده تحویل به پیک» بزند
        public static DateTime ReadyDeadline(DateTime slotEndUtc, int minDeliveryMinutes) =>
            LatestPickup(slotEndUtc, minDeliveryMinutes);

        // مهلت تأیید فروشنده: min(زمان پرداخت + مدت تأیید، پایان بازه - حداقل زمان رساندن - ۱۰ دقیقه)
        public static DateTime SellerConfirmDeadline(DateTime paidAtUtc, DateTime slotEndUtc, int confirmMinutes, int minDeliveryMinutes)
        {
            var byDuration = paidAtUtc.AddMinutes(Math.Max(1, confirmMinutes));
            var bySlot = slotEndUtc.AddMinutes(-(Math.Max(0, minDeliveryMinutes) + 10));
            return byDuration < bySlot ? byDuration : bySlot;
        }

        // آخرین ساعتی که فروشنده می‌تواند کالا را به پیک بسپارد تا هنوز وقت رساندن داخل بازه باشد.
        public static DateTime LatestPickup(DateTime slotEndUtc, int minDeliveryMinutes) =>
            slotEndUtc.AddMinutes(-Math.Max(0, minDeliveryMinutes));

        // ساعت پیش‌فرض تحویل به پیک: کمی قبل از شروع بازه، ولی هرگز زودتر از «الان + ۱۵ دقیقه».
        public static DateTime DefaultPickup(DateTime slotStartUtc, DateTime nowUtc, int pickupLeadMinutes)
        {
            var ideal = slotStartUtc.AddMinutes(-Math.Max(0, pickupLeadMinutes));
            var earliest = nowUtc.AddMinutes(15);
            return ideal > earliest ? ideal : earliest;
        }

        public static string FormatTime(TimeSpan time) => $"{time.Hours:D2}:{time.Minutes:D2}";

        public static bool TryParseTime(string value, out TimeSpan time)
        {
            time = default;
            if (string.IsNullOrWhiteSpace(value) || !TimeSpan.TryParseExact(value.Trim(), @"hh\:mm", null, out time))
                return false;
            return time >= TimeSpan.Zero && time < TimeSpan.FromHours(24);
        }
    }
}
