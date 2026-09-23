using System;
using Application.Common.Enumerable;

namespace Application.Services.ReminderSrvs.ReminderSrv
{
    public enum ReminderNotificationMoment
    {
        None = 0,
        SevenDaysBefore = 1,
        OneDayBefore = 2,
        OneDayAfter = 3,
        /// <summary>چرخه‌ی روزانه/هفتگی: فاصله‌ی بین دو نوبت خودش کوتاه است، پس فقط دقیقاً روز نوبت اطلاع داده می‌شود (بدون یادآوری چند روز قبل/بعد).</summary>
        OnTheDay = 4
    }

    public static class ReminderScheduleCalculator
    {
        /// <summary>
        /// چرخه‌ی ماهانه (رفتار قبلی، بدون تغییر — یادآور واکسن): هر Resolve سه‌آرگومانه دقیقاً همین را صدا می‌زند.
        /// چرخه‌ی روزانه/هفتگی به <see cref="Resolve(DateTime, int, ReminderCycleUnitEnum, DateTime)"/> منتقل شده.
        /// </summary>
        public static ReminderNotificationMoment Resolve(
            DateTime startDate,
            int cycleMonths,
            DateTime today) =>
            ResolveMonthly(startDate, cycleMonths, today);

        /// <summary>نقطه‌ی ورود عمومی؛ واحد چرخه را از <see cref="Entities.Entities.ReminderCycle.UnitId"/> بگیرید.</summary>
        public static ReminderNotificationMoment Resolve(
            DateTime startDate,
            int cycleCount,
            ReminderCycleUnitEnum unit,
            DateTime today)
        {
            if (cycleCount <= 0)
                return ReminderNotificationMoment.None;

            return unit switch
            {
                ReminderCycleUnitEnum.Month => ResolveMonthly(startDate, cycleCount, today),
                ReminderCycleUnitEnum.Week => ResolveByDayStep(startDate, cycleCount * 7, today),
                ReminderCycleUnitEnum.Day => ResolveByDayStep(startDate, cycleCount, today),
                _ => ReminderNotificationMoment.None
            };
        }

        /// <summary>چرخه‌ای که فاصله‌اش بین نوبت‌ها تعداد روز ثابت است (روزانه/هفتگی): یادآوری فقط دقیقاً روز نوبت.</summary>
        private static ReminderNotificationMoment ResolveByDayStep(DateTime startDate, int stepDays, DateTime today)
        {
            if (stepDays <= 0)
                return ReminderNotificationMoment.None;

            var normalizedStartDate = startDate.Date;
            var normalizedToday = today.Date;
            if (normalizedToday < normalizedStartDate)
                return ReminderNotificationMoment.None;

            var daysSinceStart = (normalizedToday - normalizedStartDate).Days;
            return daysSinceStart % stepDays == 0
                ? ReminderNotificationMoment.OnTheDay
                : ReminderNotificationMoment.None;
        }

        private static ReminderNotificationMoment ResolveMonthly(
            DateTime startDate,
            int cycleMonths,
            DateTime today)
        {
            if (cycleMonths <= 0)
                return ReminderNotificationMoment.None;

            var normalizedStartDate = startDate.Date;
            var normalizedToday = today.Date;
            var occurrenceIndex = 0;
            var occurrenceDate = normalizedStartDate;

            while (occurrenceDate.AddDays(1) < normalizedToday)
            {
                occurrenceIndex++;
                occurrenceDate = normalizedStartDate.AddMonths(checked(occurrenceIndex * cycleMonths));
            }

            while (occurrenceDate <= normalizedToday.AddDays(7))
            {
                if (normalizedToday == occurrenceDate.AddDays(-7))
                    return ReminderNotificationMoment.SevenDaysBefore;

                if (normalizedToday == occurrenceDate.AddDays(-1))
                    return ReminderNotificationMoment.OneDayBefore;

                if (normalizedToday == occurrenceDate.AddDays(1))
                    return ReminderNotificationMoment.OneDayAfter;

                occurrenceIndex++;
                occurrenceDate = normalizedStartDate.AddMonths(checked(occurrenceIndex * cycleMonths));
            }

            return ReminderNotificationMoment.None;
        }
    }
}
