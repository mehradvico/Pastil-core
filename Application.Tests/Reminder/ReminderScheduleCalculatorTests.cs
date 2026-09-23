using Application.Common.Enumerable;
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

        // ==================== چرخه‌ی روزانه/هفتگی ====================

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Resolve_InvalidDailyCycle_DoesNotSchedule(int cycleCount)
        {
            var result = ReminderScheduleCalculator.Resolve(
                new DateTime(2026, 8, 20),
                cycleCount,
                ReminderCycleUnitEnum.Day,
                new DateTime(2026, 8, 20));

            Assert.Equal(ReminderNotificationMoment.None, result);
        }

        [Fact]
        public void Resolve_Daily_FiresOnTheStartDateAndEveryDayAfter()
        {
            var startDate = new DateTime(2026, 8, 20);

            Assert.Equal(ReminderNotificationMoment.OnTheDay, ReminderScheduleCalculator.Resolve(startDate, 1, ReminderCycleUnitEnum.Day, startDate));
            Assert.Equal(ReminderNotificationMoment.OnTheDay, ReminderScheduleCalculator.Resolve(startDate, 1, ReminderCycleUnitEnum.Day, startDate.AddDays(1)));
            Assert.Equal(ReminderNotificationMoment.OnTheDay, ReminderScheduleCalculator.Resolve(startDate, 1, ReminderCycleUnitEnum.Day, startDate.AddDays(40)));
        }

        [Fact]
        public void Resolve_Daily_NeverFiresBeforeTheStartDate()
        {
            var startDate = new DateTime(2026, 8, 20);

            var result = ReminderScheduleCalculator.Resolve(startDate, 1, ReminderCycleUnitEnum.Day, startDate.AddDays(-1));

            Assert.Equal(ReminderNotificationMoment.None, result);
        }

        [Theory]
        [InlineData(2, 2026, 8, 20, ReminderNotificationMoment.OnTheDay)]
        [InlineData(2, 2026, 8, 21, ReminderNotificationMoment.None)]
        [InlineData(2, 2026, 8, 22, ReminderNotificationMoment.OnTheDay)]
        [InlineData(3, 2026, 8, 23, ReminderNotificationMoment.OnTheDay)]
        [InlineData(3, 2026, 8, 24, ReminderNotificationMoment.None)]
        public void Resolve_EveryNDays_OnlyFiresOnOccurrenceDays(
            int cycleCount,
            int year,
            int month,
            int day,
            ReminderNotificationMoment expected)
        {
            var result = ReminderScheduleCalculator.Resolve(
                new DateTime(2026, 8, 20),
                cycleCount,
                ReminderCycleUnitEnum.Day,
                new DateTime(year, month, day));

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(2026, 8, 20, ReminderNotificationMoment.OnTheDay)]
        [InlineData(2026, 8, 21, ReminderNotificationMoment.None)]
        [InlineData(2026, 8, 26, ReminderNotificationMoment.None)]
        [InlineData(2026, 8, 27, ReminderNotificationMoment.OnTheDay)]
        [InlineData(2026, 9, 3, ReminderNotificationMoment.OnTheDay)]
        public void Resolve_Weekly_FiresExactlySevenDaysApart(
            int year,
            int month,
            int day,
            ReminderNotificationMoment expected)
        {
            var result = ReminderScheduleCalculator.Resolve(
                new DateTime(2026, 8, 20),
                1,
                ReminderCycleUnitEnum.Week,
                new DateTime(year, month, day));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Resolve_WithMonthUnit_MatchesTheThreeArgumentOverloadExactly()
        {
            var startDate = new DateTime(2026, 1, 31);
            var today = new DateTime(2026, 3, 24);

            var legacy = ReminderScheduleCalculator.Resolve(startDate, 1, today);
            var generalized = ReminderScheduleCalculator.Resolve(startDate, 1, ReminderCycleUnitEnum.Month, today);

            Assert.Equal(ReminderNotificationMoment.SevenDaysBefore, legacy);
            Assert.Equal(legacy, generalized);
        }

        [Fact]
        public void Resolve_UnknownUnit_DoesNotSchedule()
        {
            var result = ReminderScheduleCalculator.Resolve(
                new DateTime(2026, 8, 20),
                1,
                (ReminderCycleUnitEnum)99,
                new DateTime(2026, 8, 20));

            Assert.Equal(ReminderNotificationMoment.None, result);
        }
    }
}
