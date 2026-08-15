using System;

namespace Application.Services.ReminderSrvs.ReminderSrv
{
    public enum ReminderNotificationMoment
    {
        None = 0,
        SevenDaysBefore = 1,
        OneDayBefore = 2,
        OneDayAfter = 3
    }

    public static class ReminderScheduleCalculator
    {
        public static ReminderNotificationMoment Resolve(
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
