using Application.Services.ReminderSrvs.ReminderSrv;
using Xunit;

namespace Application.Tests.Reminder
{
    public class ReminderScheduleCalculatorTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Resolve_InvalidCycle_DoesNotSchedule(int cycleMonths)
        {
            var result = ReminderScheduleCalculator.Resolve(
                new DateTime(2026, 8, 20),
                cycleMonths,
                new DateTime(2026, 8, 13));

            Assert.Equal(ReminderNotificationMoment.None, result);
        }

        [Theory]
        [InlineData(2026, 8, 13, ReminderNotificationMoment.SevenDaysBefore)]
        [InlineData(2026, 8, 19, ReminderNotificationMoment.OneDayBefore)]
        [InlineData(2026, 8, 21, ReminderNotificationMoment.OneDayAfter)]
        [InlineData(2026, 8, 15, ReminderNotificationMoment.None)]
        public void Resolve_FirstOccurrence_ReturnsExpectedMoment(
            int year,
            int month,
            int day,
            ReminderNotificationMoment expected)
        {
            var result = ReminderScheduleCalculator.Resolve(
                new DateTime(2026, 8, 20),
                1,
                new DateTime(year, month, day));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Resolve_RecurringOccurrence_IsCalculatedFromOriginalStartDate()
        {
            var result = ReminderScheduleCalculator.Resolve(
                new DateTime(2026, 1, 31),
                1,
                new DateTime(2026, 3, 24));

            Assert.Equal(ReminderNotificationMoment.SevenDaysBefore, result);
        }

        [Fact]
        public void Resolve_PastReminder_ContinuesRecurringSchedule()
        {
            var result = ReminderScheduleCalculator.Resolve(
                new DateTime(2020, 8, 20),
                12,
                new DateTime(2026, 8, 19));

            Assert.Equal(ReminderNotificationMoment.OneDayBefore, result);
        }
    }
}
