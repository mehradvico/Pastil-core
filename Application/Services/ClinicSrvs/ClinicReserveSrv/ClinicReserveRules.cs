using System;
using System.Linq;

namespace Application.Services.ClinicSrvs.ClinicReserveSrv
{
    /// <summary>وضعیت رزرو کلینیک؛ مستقل از جدول Codes (ستون int) تا به داده‌ی اولیه‌ی دیتابیس وابسته نباشد.</summary>
    public enum ClinicReserveStatusEnum
    {
        Registered = 0,
        Paid = 1,
        Complete = 2,
    }

    /// <summary>تصمیم کلینیک درباره‌ی رزروِ پرداخت‌شده (همان الگوی پانسیون).</summary>
    public enum ClinicReserveOwnerDecisionEnum
    {
        NotRequired = 0,
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Expired = 4,
    }

    /// <summary>قواعد خالص رزرو کلینیک (بدون دیتابیس) تا تست‌پذیر باشند.</summary>
    public static class ClinicReserveRules
    {
        /// <summary>سقف تعداد بسته در یک رزرو و سقف مجموع مدت (دقیقه) یک نوبت.</summary>
        public const int MaxPackages = 5;
        public const int MaxTotalMinutes = 12 * 60;

        /// <summary>اگر خدمت مدت‌زمان نداشته باشد.</summary>
        public const int DefaultDurationMinutes = 60;
        /// <summary>فاصله‌ی شروع نوبت‌های پیشنهادی در لیست زمان‌های آزاد.</summary>
        public const int SlotStepMinutes = 30;
        /// <summary>رزرو باید دست‌کم این‌قدر بعد از «الان» شروع شود.</summary>
        public static readonly TimeSpan MinLead = TimeSpan.FromHours(1);
        /// <summary>رزروِ پرداخت‌نشده تا این مدت ظرفیت را نگه می‌دارد (تا کاربر وسط پرداخت جا را از دست ندهد).</summary>
        public static readonly TimeSpan UnpaidHold = TimeSpan.FromMinutes(15);

        /// <summary>مهلت پاسخ کلینیک از لحظه‌ی پرداخت.</summary>
        public static readonly TimeSpan DecisionWindow = TimeSpan.FromHours(12);
        public static readonly TimeSpan MinWindow = TimeSpan.FromMinutes(30);
        /// <summary>مهلت پاسخ باید دست‌کم این‌قدر قبل از شروع نوبت تمام شود.</summary>
        public static readonly TimeSpan StartBuffer = TimeSpan.FromMinutes(30);

        /// <summary>لغو توسط کاربر تا این مدت قبل از نوبت رایگان است (برگشت کل مبلغ به کیف پول).</summary>
        public static readonly TimeSpan FreeCancelWindow = TimeSpan.FromHours(12);

        /// <summary>شماره‌ی روز هفته مثل WeekDay.Number: شنبه ۱ ... جمعه ۷.</summary>
        public static int WeekDayNumber(DateTime date) => date.DayOfWeek switch
        {
            DayOfWeek.Saturday => 1,
            DayOfWeek.Sunday => 2,
            DayOfWeek.Monday => 3,
            DayOfWeek.Tuesday => 4,
            DayOfWeek.Wednesday => 5,
            DayOfWeek.Thursday => 6,
            DayOfWeek.Friday => 7,
            _ => throw new ArgumentOutOfRangeException(nameof(date)),
        };

        public static bool TryParseTime(string value, out TimeSpan time) =>
            TimeSpan.TryParseExact(value?.Trim(), @"h\:mm", System.Globalization.CultureInfo.InvariantCulture, out time) && time < TimeSpan.FromHours(24);

        public static int EffectiveDuration(int serviceDurationMinutes) =>
            serviceDurationMinutes > 0 ? serviceDurationMinutes : DefaultDurationMinutes;

        /// <summary>ظرفیت هم‌زمان: ردیف ساعت کاری، و اگر کلینیک سقف کلی دارد کمینه‌ی آن‌ها.</summary>
        public static int EffectiveCapacity(int timeCapacity, int clinicMaxConcurrentPets) =>
            clinicMaxConcurrentPets > 0 ? Math.Min(timeCapacity, clinicMaxConcurrentPets) : timeCapacity;

        public static bool Overlaps(DateTime startA, DateTime endA, DateTime startB, DateTime endB) =>
            startA < endB && endA > startB;

        /// <summary>
        /// بیشینه‌ی تعداد رزروهای هم‌زمان (از میان رزروهای موجود) داخل بازه‌ی [start, end). نوبت جدید فقط وقتی جا دارد که این عدد کمتر از ظرفیت باشد.
        /// پایان یک رزرو و شروع رزرو بعدی در یک لحظه هم‌پوشانی حساب نمی‌شود.
        /// </summary>
        public static int MaxConcurrency(System.Collections.Generic.IEnumerable<(DateTime Start, DateTime End)> existing, DateTime start, DateTime end)
        {
            var events = new System.Collections.Generic.List<(DateTime Time, int Delta)>();
            foreach (var (s, e) in existing)
            {
                var from = s > start ? s : start;
                var to = e < end ? e : end;
                if (from >= to)
                    continue;
                events.Add((from, 1));
                events.Add((to, -1));
            }
            events.Sort((a, b) => a.Time != b.Time ? a.Time.CompareTo(b.Time) : a.Delta.CompareTo(b.Delta));
            int current = 0, max = 0;
            foreach (var (_, delta) in events)
            {
                current += delta;
                if (current > max)
                    max = current;
            }
            return max;
        }

        /// <summary>سهم یک بسته از مبلغ آنلاین: پیش‌پرداخت، یا کل قیمت اگر پیش‌پرداخت ندارد/بیشتر از قیمت است.</summary>
        public static double Deposit(double packagePrice, double prePaymentPrice) =>
            prePaymentPrice > 0 && prePaymentPrice < packagePrice ? prePaymentPrice : packagePrice;

        /// <summary>چند بسته: (کل قیمت، مبلغ آنلاین).</summary>
        public static (double Price, double OnlineAmount) Amounts(System.Collections.Generic.IEnumerable<(double Price, double PrePayment)> packages)
        {
            double price = 0, online = 0;
            foreach (var (packagePrice, prePayment) in packages)
            {
                price += packagePrice;
                online += Deposit(packagePrice, prePayment);
            }
            return (price, online);
        }

        /// <summary>
        /// قیمت یک بسته برای یک نحوه‌ی ارائه. بسته‌ی بدون ردیف نحوه‌ی ارائه (قدیمی/ساده) همه‌ی حالت‌ها را با قیمت پایه ارائه می‌دهد؛
        /// بسته‌ای که ردیف دارد فقط حالت‌های لیست‌شده را (null = در این حالت ارائه نمی‌شود).
        /// </summary>
        public static (double Price, double PrePayment)? ResolveModePrice(
            double basePrice, double basePrePayment, System.Collections.Generic.IReadOnlyList<(int Mode, double Price, double PrePayment)> types, int mode)
        {
            if (types == null || types.Count == 0)
                return (basePrice, basePrePayment);
            foreach (var row in types)
                if (row.Mode == mode)
                    return (row.Price, row.PrePayment);
            return null;
        }

        /// <summary>
        /// تقسیم مبلغ پرداخت‌شده بین بخش‌ها به نسبت وزن و کمیسیون هر بخش با درصد خودش. هر بخش به تومان گرد می‌شود و آخرین بخش باقی‌مانده را
        /// می‌گیرد تا جمع بخش‌ها دقیقاً برابر مبلغ پرداخت باشد.
        /// </summary>
        public static System.Collections.Generic.List<(double Provider, double Site)> AllocateCommission(
            double paymentPrice, System.Collections.Generic.IReadOnlyList<(double Weight, decimal Percent)> parts)
        {
            var result = new System.Collections.Generic.List<(double Provider, double Site)>();
            var totalWeight = parts.Sum(p => (decimal)Math.Max(0, p.Weight));
            if (paymentPrice <= 0 || totalWeight <= 0)
            {
                foreach (var _ in parts)
                    result.Add((0, 0));
                return result;
            }

            decimal remaining = (decimal)paymentPrice;
            for (var i = 0; i < parts.Count; i++)
            {
                var paid = i == parts.Count - 1
                    ? remaining
                    : Math.Min(remaining, Math.Round((decimal)paymentPrice * (decimal)Math.Max(0, parts[i].Weight) / totalWeight, 0, MidpointRounding.AwayFromZero));
                remaining -= paid;
                var percent = Math.Clamp(parts[i].Percent, 0m, 100m);
                var site = Math.Round(paid * percent / 100m, 0, MidpointRounding.AwayFromZero);
                result.Add(((double)(paid - site), (double)site));
            }
            return result;
        }

        /// <summary>مبلغ پرداخت آنلاین بعد از تخفیف (هرگز منفی نمی‌شود).</summary>
        public static double PaymentAfterRebate(double onlineAmount, double rebatePrice) =>
            Math.Max(0, onlineAmount - Math.Max(0, rebatePrice));

        /// <summary>درصد کمیسیون: مقدار خود خدمت، و اگر صفر بود مقدار پیش‌فرض کلینیک.</summary>
        public static decimal ResolveCommission(decimal servicePercent, decimal clinicPercent) =>
            servicePercent > 0 ? servicePercent : clinicPercent;

        /// <summary>تقسیم مبلغ پرداخت‌شده به سهم سایت و سهم ارائه‌دهنده؛ گرد به تومان و مجموع دقیقاً برابر مبلغ.</summary>
        public static (double Provider, double Site) Split(double paymentPrice, decimal commissionPercent)
        {
            var percent = Math.Clamp(commissionPercent, 0m, 100m);
            var site = Math.Round((decimal)paymentPrice * percent / 100m, 0, MidpointRounding.AwayFromZero);
            var provider = (decimal)paymentPrice - site;
            return ((double)provider, (double)site);
        }

        public static DateTime Deadline(DateTime now, DateTime startAt)
        {
            var deadline = now + DecisionWindow;
            var beforeStart = startAt - StartBuffer;
            if (beforeStart < deadline)
                deadline = beforeStart;
            var floor = now + MinWindow;
            return deadline < floor ? floor : deadline;
        }

        public static bool IsPending(int decision) => decision == (int)ClinicReserveOwnerDecisionEnum.Pending;

        public static bool CanDecide(bool isReserved, bool isCancel, int decision) =>
            isReserved && !isCancel && IsPending(decision);

        public static bool IsOverdue(int decision, DateTime? deadline, DateTime now) =>
            IsPending(decision) && deadline.HasValue && deadline.Value <= now;

        public static bool CanComplete(int decision) =>
            decision == (int)ClinicReserveOwnerDecisionEnum.NotRequired ||
            decision == (int)ClinicReserveOwnerDecisionEnum.Approved;

        public static bool IsFreeCancel(DateTime now, DateTime doDate) => doDate - now >= FreeCancelWindow;

        public static string RefundLedgerName(long reserveId) => $"ClinicReserveRefund:{reserveId}";
    }
}
