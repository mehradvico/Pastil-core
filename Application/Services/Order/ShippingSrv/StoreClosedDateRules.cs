using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Application.Services.Order.ShippingSrv
{
    /// <summary>تاریخ‌های تعطیلی فروشگاه به روز تقویمی تهران مربوط‌اند، نه UTC.</summary>
    public static class StoreClosedDateRules
    {
        public const int CalendarHorizonDays = 90;
        public const int MaxBatchDates = 31;

        public static DateTime TehranToday(DateTime utcNow) => ShippingSlotRules.ToTehran(utcNow).Date;

        public static bool TryParse(string value, out DateTime date)
        {
            if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsed))
            {
                date = parsed.Date;
                return true;
            }
            date = default;
            return false;
        }

        public static bool CanChange(DateTime date, DateTime today) =>
            date.Date >= today.Date && date.Date <= today.Date.AddDays(CalendarHorizonDays);

        public static bool IsClosed(DateTime date, IReadOnlyCollection<DateTime> closedDates) =>
            closedDates != null && closedDates.Contains(date.Date);
    }
}
