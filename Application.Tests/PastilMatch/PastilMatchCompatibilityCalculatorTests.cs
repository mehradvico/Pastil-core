using Application.Services.PastilMatchSrvs.PastilMatchSuggestionSrv;
using Xunit;

namespace Application.Tests.PastilMatch;

public class PastilMatchCompatibilityCalculatorTests
{
    [Fact]
    public void IdenticalProfilesAtSameLocation_ReturnOneHundredPercent()
    {
        var birthday = new DateTime(2022, 5, 10);

        var result = PastilMatchCompatibilityCalculator.Calculate(
            birthday,
            birthday,
            1,
            1,
            new long[] { 10 },
            new long[] { 10 },
            new long[] { 10124, 10125 },
            new long[] { 10124, 10125 },
            10116,
            10116,
            10121,
            10121,
            51.389,
            35.6892,
            51.389,
            35.6892
        );

        Assert.Equal(100, result.TotalPercent);
        Assert.Equal(100, result.GoalsPercent);
        Assert.Equal(100, result.DistancePercent);
        Assert.Equal(100, result.AgePercent);
        Assert.Equal(100, result.BreedPercent);
    }

    [Fact]
    public void WeakCompatibility_CanFallBelowThirtyPercent()
    {
        var result = PastilMatchCompatibilityCalculator.Calculate(
            new DateTime(2016, 1, 1),
            new DateTime(2024, 1, 1),
            1,
            1,
            new long[] { 10 },
            new long[] { 20 },
            new long[] { 10124 },
            new long[] { 10127 },
            10114,
            10118,
            10119,
            10123,
            51.389,
            35.6892,
            51.389,
            36.1392
        );

        Assert.InRange(result.TotalPercent, 0, 30);
    }

    [Fact]
    public void Distance_UsesKilometers()
    {
        var distance = PastilMatchCompatibilityCalculator
            .CalculateDistanceInKilometers(
                51.389,
                35.6892,
                51.389,
                36.1392
            );

        Assert.NotNull(distance);
        Assert.InRange(distance.Value, 49D, 51D);
    }
}
