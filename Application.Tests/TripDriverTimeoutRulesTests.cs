using Application.Services.ProductSrvs.WalletSrv;
using Application.Services.TripSrv.TripSrv;
using System;
using Xunit;

namespace Application.Tests;

public class TripDriverTimeoutRulesTests
{
    private static readonly DateTime Created = new(2026, 10, 17, 10, 0, 0);

    [Fact]
    public void InstantTrip_CancelsThirtyMinutesAfterRequest()
    {
        Assert.True(TripDriverTimeoutRules.IsInstant(null, null));
        var departure = TripDriverTimeoutRules.Departure(null, null, Created);
        Assert.Null(departure);
        Assert.Equal(Created.AddMinutes(30), TripDriverTimeoutRules.CancelAt(Created, departure));
    }

    [Fact]
    public void ReservationDaysAhead_CancelsThirtyMinutesBeforeDeparture()
    {
        var departAt = Created.AddDays(3);
        var departure = TripDriverTimeoutRules.Departure(departAt, null, departAt);
        Assert.Equal(departAt.AddMinutes(-30), TripDriverTimeoutRules.CancelAt(Created, departure));
    }

    [Fact]
    public void ReservationBookedShortlyBeforeDeparture_StillGetsThirtyMinutesForAdmin()
    {
        var departAt = Created.AddMinutes(40);
        Assert.Equal(Created.AddMinutes(30), TripDriverTimeoutRules.CancelAt(Created, TripDriverTimeoutRules.Departure(departAt, null, departAt)));
    }

    [Fact]
    public void ServiceOccurrence_UsesTripStartAsDeparture_SoNextWeekIsNotCancelledImmediately()
    {
        var nextWeek = Created.AddDays(7);
        Assert.False(TripDriverTimeoutRules.IsInstant(null, 12));
        var departure = TripDriverTimeoutRules.Departure(null, 12, nextWeek);
        Assert.Equal(nextWeek, departure);
        Assert.Equal(nextWeek.AddMinutes(-30), TripDriverTimeoutRules.CancelAt(Created, departure));
    }

    [Fact]
    public void AdminNoticeAfterTenMinutes()
    {
        Assert.Equal(TimeSpan.FromMinutes(10), TripDriverTimeoutRules.AdminNoticeDelay);
    }

    [Fact]
    public void TripCancelRefund_HasReadableWalletText()
    {
        var text = WalletDescriptionRules.Describe(
            new WalletDescribeInput { Name = WalletDescriptionRules.TripCancelRefundName(55), IsIncrease = true }, null);
        Assert.Equal(string.Format(Resource.Pattern.WalletReasonCancelRefundFor, string.Format(Resource.Pattern.WalletTargetTrip, 55)), text);
    }
}
