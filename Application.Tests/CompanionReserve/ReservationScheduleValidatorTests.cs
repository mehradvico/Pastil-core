using Application.Common.Helpers;
using Xunit;

namespace Application.Tests.CompanionReserve;

public class ReservationScheduleValidatorTests
{
    [Theory]
    [InlineData("09:00", "10:00", true)]
    [InlineData("09:00", "09:00", false)]
    [InlineData("10:00", "09:00", false)]
    [InlineData("9", "10:00", false)]
    public void TryGetServiceTimeRange_ValidatesConfiguredRange(
        string start,
        string end,
        bool expected)
    {
        var result = ReservationScheduleValidator.TryGetServiceTimeRange(
            start,
            end,
            out _,
            out _);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(9, 10, 9, 10, true)]
    [InlineData(9, 11, 10, 12, true)]
    [InlineData(9, 10, 10, 11, false)]
    [InlineData(10, 11, 9, 10, false)]
    public void HasTimeRangeOverlap_DetectsOnlyRealOverlap(
        int firstStart,
        int firstEnd,
        int secondStart,
        int secondEnd,
        bool expected)
    {
        var result = ReservationScheduleValidator.HasTimeRangeOverlap(
            TimeSpan.FromHours(firstStart),
            TimeSpan.FromHours(firstEnd),
            TimeSpan.FromHours(secondStart),
            TimeSpan.FromHours(secondEnd));

        Assert.Equal(expected, result);
    }
}
