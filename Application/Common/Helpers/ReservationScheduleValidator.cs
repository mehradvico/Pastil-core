using System;
using System.Globalization;

namespace Application.Common.Helpers
{
    public static class ReservationScheduleValidator
    {
        public static bool IsPansionModeValid(bool? isSchool, bool hasHourlyInputs, bool hasDailyInputs)
        {
            if (!isSchool.HasValue || hasHourlyInputs == hasDailyInputs)
            {
                return false;
            }

            return isSchool.Value ? hasHourlyInputs : hasDailyInputs;
        }

        public static bool IsDateInPast(DateTime date, DateTime now)
        {
            return date.Date < now.Date;
        }

        public static bool IsWeekDayMatch(DateTime date, string weekDayLabel)
        {
            return !string.IsNullOrWhiteSpace(weekDayLabel) &&
                   string.Equals(
                       date.DayOfWeek.ToString(),
                       weekDayLabel.Trim(),
                       StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryGetServiceStartDateTime(
            DateTime serviceDate,
            string startTime,
            out DateTime startDateTime)
        {
            startDateTime = default;
            if (!TimeSpan.TryParseExact(
                    startTime,
                    "hh\\:mm",
                    CultureInfo.InvariantCulture,
                    out var parsedStartTime))
            {
                return false;
            }

            startDateTime = serviceDate.Date.Add(parsedStartTime);
            return true;
        }
    }
}
