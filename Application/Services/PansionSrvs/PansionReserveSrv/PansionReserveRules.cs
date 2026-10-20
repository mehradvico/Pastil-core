using System;
using System.Globalization;

namespace Application.Services.PansionSrvs.PansionReserveSrv
{
    // قواعد قیمت و زمان رزرو پانسیون/مهد؛ همه‌ی اعداد مالی فقط از همین‌جا و از روی داده‌ی سرور ساخته می‌شوند، نه از بدنه‌ی درخواست
    public static class PansionReserveRules
    {
        // تعرفه‌ی پانسیون «هر شب» است: ورود ۲۰ و خروج ۲۱ = ۱ شب. قبلاً روزها شمرده می‌شد ((to-from)+1) و هر رزرو یک شب اضافه می‌گرفت.
        public static int NightCount(DateTime fromDate, DateTime toDate) => (toDate.Date - fromDate.Date).Days;

        // مهد ساعتی است و ساعت ناقص یک ساعت کامل حساب می‌شود (۰۸:۰۰ تا ۰۹:۳۰ = ۲ ساعت)
        public static int DaycareHourCount(TimeSpan start, TimeSpan end) => (int)Math.Ceiling((end - start).TotalHours);

        // دو اقامت شبانه فقط وقتی تداخل دارند که شبی مشترک داشته باشند؛ روز خروج یکی می‌تواند روز ورود دیگری باشد
        public static bool StaysOverlap(DateTime firstFrom, DateTime firstTo, DateTime secondFrom, DateTime secondTo) =>
            firstFrom.Date < secondTo.Date && secondFrom.Date < firstTo.Date;

        // OpenHour/CloseHour متن آزادِ پنل است («08:00»، «8»، «24:00»)؛ اگر قابل خواندن نبود چک ساعت کاری انجام نمی‌شود
        public static bool TryParseHour(string value, out TimeSpan hour)
        {
            hour = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var text = value.Trim();
            if (text == "24" || text == "24:00")
            {
                hour = TimeSpan.FromHours(24);
                return true;
            }

            if (int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var wholeHour) && wholeHour >= 0 && wholeHour < 24)
            {
                hour = TimeSpan.FromHours(wholeHour);
                return true;
            }

            return TimeSpan.TryParseExact(text, new[] { "hh\\:mm", "h\\:mm" }, CultureInfo.InvariantCulture, out hour) &&
                   hour >= TimeSpan.Zero && hour < TimeSpan.FromHours(24);
        }

        public enum RepriceResult { Unchanged, Changed, Invalid }

        // شروع پرداخت: مبلغ پایه دوباره از تعرفه‌ی فعلی مرکز × شب/ساعت ساخته می‌شود و همان به درگاه می‌رود (رزروهای قدیمی‌ترِ پرداخت‌نشده
        // با فرمول «روز + ۱» هم این‌جا اصلاح می‌شوند). تخفیفِ اعمال‌شده دست نمی‌خورد؛ اگر با پایه‌ی جدید نخواند، اعتبارسنجی تخفیف ردش می‌کند.
        public static RepriceResult Reprice(Entities.Entities.PansionField.PansionReserve reserve, Entities.Entities.PansionField.Pansion pansion)
        {
            if (reserve == null || pansion == null)
                return RepriceResult.Invalid;

            int dayCount = 0, hourCount = 0;
            double basePrice;
            if (reserve.FromDate.HasValue && reserve.ToDate.HasValue)
            {
                dayCount = NightCount(reserve.FromDate.Value, reserve.ToDate.Value);
                if (dayCount <= 0)
                    return RepriceResult.Invalid;
                basePrice = pansion.PansionPrice * dayCount;
            }
            else if (TimeSpan.TryParseExact(reserve.StartTime, "hh\\:mm", CultureInfo.InvariantCulture, out var start) &&
                     TimeSpan.TryParseExact(reserve.EndTime, "hh\\:mm", CultureInfo.InvariantCulture, out var end) && end > start)
            {
                hourCount = DaycareHourCount(start, end);
                basePrice = pansion.SchoolPrice * hourCount;
            }
            else
            {
                return RepriceResult.Invalid;
            }

            if (basePrice <= 0)
                return RepriceResult.Invalid;
            if (Math.Abs(basePrice - reserve.Price) < 0.01 && reserve.DayCount == dayCount && reserve.HourCount == hourCount)
                return RepriceResult.Unchanged;

            reserve.Price = basePrice;
            reserve.DayCount = dayCount;
            reserve.HourCount = hourCount;
            reserve.PaymentPrice = Math.Max(0, basePrice - reserve.RebatePrice);
            return RepriceResult.Changed;
        }

        public static bool IsWithinWorkingHours(string openHour, string closeHour, TimeSpan start, TimeSpan end)
        {
            if (!TryParseHour(openHour, out var open) || !TryParseHour(closeHour, out var close) || close <= open)
                return true;
            return start >= open && end <= close;
        }
    }
}
