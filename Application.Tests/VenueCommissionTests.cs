using Application.Common.Helpers;
using Xunit;

namespace Application.Tests;

public class VenueCommissionTests
{
    [Theory]
    [InlineData(0, 10, 10)]
    [InlineData(15, 10, 15)]
    [InlineData(0, 0, 0)]
    [InlineData(7.5, 0, 7.5)]
    public void Resolve_ItemPercentWins_ZeroFallsBackToVenue(double item, double venue, double expected)
    {
        Assert.Equal((decimal)expected, VenueCommission.Resolve((decimal)item, (decimal)venue));
    }
}
