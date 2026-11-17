using System;
using System.Collections.Generic;
using System.Linq;

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

        // آخرین ساعتی که فروشنده باید «آماده تحویل به پیک» بزند. چون pickup.deadline هیچ‌وقت کمتر از «الان + MinPickupFromNowMinutes»
        // نمی‌شود، دکمه باید MinPickupFromNowMinutes زودتر از «آخرین ساعت تحویل به پیک» بسته شود؛ وگرنه فروشنده مهلتی را می‌بیند
        // که در نیم‌ساعت آخرش سرور ساخت سفر را رد می‌کند.
        public static DateTime ReadyDeadline(DateTime slotEndUtc, int minDeliveryMinutes) =>
            LatestPickup(slotEndUtc, minDeliveryMinutes).AddMinutes(-MinPickupFromNowMinutes);

        // مهلت تأیید فروشنده: min(مدت تأیید از لحظه‌ی پرداخت (فقط در «ساعت کاری فروشگاه»)، مهلت «آماده تحویل» - ۵ دقیقه)
        // storeHours خالی = فروشگاه بدون ساعت کاری ثبت‌شده = همیشه باز (مثل قبل: پرداخت + confirmMinutes).
        public static DateTime SellerConfirmDeadline(
            DateTime paidAtUtc, DateTime slotEndUtc, int confirmMinutes, int minDeliveryMinutes,
            IReadOnlyCollection<(DayOfWeek Day, TimeSpan Open, TimeSpan Close)> storeHours = null)
        {
            var byDuration = AddWorkingMinutes(paidAtUtc, Math.Max(1, confirmMinutes), storeHours);
            var bySlot = ReadyDeadline(slotEndUtc, minDeliveryMinutes).AddMinutes(-5);
            return byDuration < bySlot ? byDuration : bySlot;
        }

        // «minutes دقیقه» را فقط از زمان‌های باز بودن فروشگاه (به وقت تهران) مصرف می‌کند. سفارشی که ساعت ۲۲ ثبت شده برای فروشگاهی
        // با ساعت کاری ۰۹ تا ۱۸ مهلتش فردا ساعت ۱۰ تمام می‌شود، نه ۲۳. بدون ساعت کاری (یا بدون روز باز در ۱۴ روز آینده) = جمع ساده.
        public static DateTime AddWorkingMinutes(
            DateTime startUtc, int minutes, IReadOnlyCollection<(DayOfWeek Day, TimeSpan Open, TimeSpan Close)> storeHours)
        {
            var plain = startUtc.AddMinutes(minutes);
            var intervals = (storeHours ?? Array.Empty<(DayOfWeek Day, TimeSpan Open, TimeSpan Close)>())
                .Where(item => item.Close > item.Open)
                .ToList();
            if (intervals.Count == 0)
                return plain;

            var local = ToTehran(startUtc);
            var date = local.Date;
            var timeOfDay = local.TimeOfDay;
            var remaining = TimeSpan.FromMinutes(minutes);
            for (var day = 0; day < 15; day++)
            {
                foreach (var interval in intervals.Where(item => item.Day == date.DayOfWeek).OrderBy(item => item.Open))
                {
                    if (timeOfDay >= interval.Close)
                        continue;
                    var from = timeOfDay > interval.Open ? timeOfDay : interval.Open;
                    var available = interval.Close - from;
                    if (remaining <= available)
                        return ToUtc(date, from + remaining);
                    remaining -= available;
                }
                date = date.AddDays(1);
                timeOfDay = TimeSpan.Zero;
            }
            return plain;
        }

        // فاصله‌ی «موعد ارسال رسید» تا یادآوری فوری، اگر فروشنده هنوز «آماده تحویل به پیک» نزده.
        public const int ReadyUrgentAfterMinutes = 30;

        // یادآوری «آماده تحویل به پیک» به فروشنده (پیامک + پوش)، با یک ستون (ReadyReminderSentAtUtc) بدون migration:
        //   ۱ = «موعد ارسال رسید» (anchorUtc = شروع بازه، یا اولین لحظه‌ی باز بودن فروشگاه بعد از آن)؛
        //   ۲ = «فقط تا ساعت … وقت دارید» وقتی ۳۰ دقیقه بعد از پیام اول هنوز آماده نشده و مهلت هنوز نرسیده.
        // فروشنده‌ای که بعد از شروع بازه (بعد از anchor + ۳۰) تأیید کرده فقط پیام اول را می‌گیرد؛ ستون یک‌بار claim می‌شود.
        // برگشت: ۰ = چیزی نفرست.
        public static int ReadyNudgeStage(DateTime anchorUtc, DateTime readyDeadlineUtc, DateTime? lastReminderUtc, DateTime nowUtc)
        {
            if (nowUtc < anchorUtc)
                return 0;
            if (lastReminderUtc == null || lastReminderUtc.Value < anchorUtc)
                return 1;
            var urgentAt = lastReminderUtc.Value.AddMinutes(ReadyUrgentAfterMinutes);
            if (lastReminderUtc.Value < anchorUtc.AddMinutes(ReadyUrgentAfterMinutes) && nowUtc >= urgentAt && urgentAt < readyDeadlineUtc)
                return 2;
            return 0;
        }

        // آخرین ساعتی که فروشنده می‌تواند کالا را به پیک بسپارد تا هنوز وقت رساندن داخل بازه باشد.
        public static DateTime LatestPickup(DateTime slotEndUtc, int minDeliveryMinutes) =>
            slotEndUtc.AddMinutes(-Math.Max(0, minDeliveryMinutes));

        // میاره pickup.deadline نزدیک را با «403 deadline_too_soon» رد می‌کند (۱۵ دقیقه از الان رد شد: درخواست پیک همه‌ی سفارش‌های
        // اکسپرس شکست می‌خورد)؛ ۳۰ دقیقه همان مقدار پیش‌فرض قدیمی ما بود که پذیرفته می‌شد.
        public const int MinPickupFromNowMinutes = 30;

        // ساعت پیش‌فرض تحویل به پیک: کمی قبل از شروع بازه، ولی هرگز زودتر از «الان + MinPickupFromNowMinutes».
        public static DateTime DefaultPickup(DateTime slotStartUtc, DateTime nowUtc, int pickupLeadMinutes)
        {
            var ideal = slotStartUtc.AddMinutes(-Math.Max(0, pickupLeadMinutes));
            var earliest = nowUtc.AddMinutes(MinPickupFromNowMinutes);
            return ideal > earliest ? ideal : earliest;
        }

        // بازه‌ی تحویل فقط برای فروشگاهی قابل ارائه است که ساعت کاری‌اش با آن بازه هم‌پوشانی دارد و پیک بتواند داخل ساعت کاری کالا را بگیرد:
        // - بازه با ساعت باز بودن همان روز هم‌پوشانی داشته باشد (فروشگاه ۱۰ تا ۱۷ ← بازه‌ی ۱۷ تا ۲۱ نه، ۹ تا ۱۳ بله)؛
        // - ساعت تحویل به پیک = بیشینه‌ی (شروع بازه − PickupLead، ساعت بازشدن) هنوز قبل از بسته‌شدن و نه دیرتر از «پایان بازه − MinDelivery» باشد.
        // بدون هیچ ردیف ساعت کاری = بدون محدودیت؛ با ردیف: روزی که ردیف ندارد تعطیل است (مثل IsStoreOpen).
        public static bool IsWithinStoreHours(
            DayOfWeek day, TimeSpan slotStart, TimeSpan slotEnd, int pickupLeadMinutes, int minDeliveryMinutes,
            IReadOnlyCollection<(DayOfWeek Day, TimeSpan Open, TimeSpan Close)> storeHours)
        {
            if (storeHours == null || storeHours.Count == 0)
                return true;
            var idealPickup = slotStart - TimeSpan.FromMinutes(Math.Max(0, pickupLeadMinutes));
            var latestPickup = slotEnd - TimeSpan.FromMinutes(Math.Max(0, minDeliveryMinutes));
            foreach (var interval in storeHours.Where(item => item.Day == day && item.Close > item.Open))
            {
                if (interval.Open >= slotEnd || interval.Close <= slotStart)
                    continue;
                var pickup = idealPickup > interval.Open ? idealPickup : interval.Open;
                if (pickup < interval.Close && pickup <= latestPickup)
                    return true;
            }
            return false;
        }

        // اولین لحظه‌ی باز بودن فروشگاه در/بعد از utc (به وقت تهران). بدون ساعت کاری (یا بدون روز باز در ۱۴ روز آینده) = همان utc.
        public static DateTime NextOpenAtOrAfter(
            DateTime utc, IReadOnlyCollection<(DayOfWeek Day, TimeSpan Open, TimeSpan Close)> storeHours)
        {
            var intervals = (storeHours ?? Array.Empty<(DayOfWeek Day, TimeSpan Open, TimeSpan Close)>())
                .Where(item => item.Close > item.Open)
                .ToList();
            if (intervals.Count == 0)
                return utc;
            var local = ToTehran(utc);
            var date = local.Date;
            var timeOfDay = local.TimeOfDay;
            for (var day = 0; day < 15; day++)
            {
                foreach (var interval in intervals.Where(item => item.Day == date.DayOfWeek).OrderBy(item => item.Open))
                {
                    if (timeOfDay >= interval.Close)
                        continue;
                    return ToUtc(date, timeOfDay > interval.Open ? timeOfDay : interval.Open);
                }
                date = date.AddDays(1);
                timeOfDay = TimeSpan.Zero;
            }
            return utc;
        }

        // اکسپرس: پیک بعد از leadMinutes می‌رسد، ولی هیچ‌وقت کمتر از حداقل مورد قبول میاره
        public static DateTime ExpressPickup(DateTime nowUtc, int leadMinutes) =>
            nowUtc.AddMinutes(Math.Max(MinPickupFromNowMinutes, leadMinutes));

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
