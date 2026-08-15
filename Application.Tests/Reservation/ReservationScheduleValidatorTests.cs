using Application.Common.Helpers;
using Xunit;

namespace Application.Tests.Reservation;

public class ReservationScheduleValidatorTests
{
    [Theory]
    [InlineData(true, true, false, true)]
    [InlineData(true, false, true, false)]
    [InlineData(false, false, true, true)]
    [InlineData(false, true, false, false)]
    [InlineData(true, true, true, false)]
    [InlineData(false, false, false, false)]
    public void IsPansionModeValid_RequiresInputsMatchingConfiguredMode(
        bool isSchool,
        bool hasHourlyInputs,
        bool hasDailyInputs,
        bool expected)
    {
        var result = ReservationScheduleValidator.IsPansionModeValid(
            isSchool,
            hasHourlyInputs,
            hasDailyInputs);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsPansionModeValid_RejectsUnconfiguredPansionMode()
    {
        Assert.False(ReservationScheduleValidator.IsPansionModeValid(null, true, false));
    }

    [Fact]
    public void IsWeekDayMatch_MatchesSelectedDateToConfiguredWeekDay()
    {
        var date = new DateTime(2026, 8, 20);

        Assert.True(ReservationScheduleValidator.IsWeekDayMatch(date, "Thursday"));
        Assert.False(ReservationScheduleValidator.IsWeekDayMatch(date, "Friday"));
    }

    [Fact]
    public void TryGetServiceStartDateTime_CombinesDateAndSelectedStartTime()
    {
        var valid = ReservationScheduleValidator.TryGetServiceStartDateTime(
            new DateTime(2026, 8, 20, 23, 10, 0),
            "17:30",
            out var result);

        Assert.True(valid);
        Assert.Equal(new DateTime(2026, 8, 20, 17, 30, 0), result);
    }

    [Theory]
    [InlineData("17")]
    [InlineData("17:30:00")]
    [InlineData("invalid")]
    public void TryGetServiceStartDateTime_RejectsInvalidTimeFormat(string startTime)
    {
        Assert.False(ReservationScheduleValidator.TryGetServiceStartDateTime(
            new DateTime(2026, 8, 20),
            startTime,
            out _));
    }

    [Fact]
    public void IsDateInPast_UsesDatePartOnly()
    {
        var now = new DateTime(2026, 8, 20, 18, 0, 0);

        Assert.True(ReservationScheduleValidator.IsDateInPast(new DateTime(2026, 8, 19, 23, 59, 0), now));
        Assert.False(ReservationScheduleValidator.IsDateInPast(new DateTime(2026, 8, 20, 1, 0, 0), now));
    }
}
