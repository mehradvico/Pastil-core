using Application.Services.PansionSrvs.PansionReserveSrv;
using Entities.Entities.PansionField;
using System;
using Xunit;

namespace Application.Tests.PreLaunchFixes;

public class PansionReserveRulesTests
{
    [Fact]
    public void NightCount_CheckInAndNextDay_IsOneNight()
    {
        // ورود ۲۰ مهر، خروج ۲۱ مهر: قبلاً ۲ روز × ۸۰۰٬۰۰۰ حساب می‌شد
        Assert.Equal(1, PansionReserveRules.NightCount(new DateTime(2026, 10, 12), new DateTime(2026, 10, 13)));
        Assert.Equal(0, PansionReserveRules.NightCount(new DateTime(2026, 10, 12), new DateTime(2026, 10, 12, 18, 0, 0)));
    }

    [Fact]
    public void StaysOverlap_CheckoutDayMayBeNextCheckin()
    {
        var a = new DateTime(2026, 10, 12);
        var b = new DateTime(2026, 10, 13);
        var c = new DateTime(2026, 10, 14);

        Assert.False(PansionReserveRules.StaysOverlap(a, b, b, c));
        Assert.True(PansionReserveRules.StaysOverlap(a, c, b, c));
    }

    [Theory]
    [InlineData("08:00", "22:00", "09:00", "11:00", true)]
    [InlineData("08:00", "22:00", "03:00", "05:00", false)]
    [InlineData("8", "22", "07:30", "09:00", false)]
    [InlineData("08:00", "24:00", "21:00", "23:30", true)]
    [InlineData("صبح", "شب", "03:00", "05:00", true)]
    [InlineData("22:00", "06:00", "03:00", "05:00", true)]
    public void WorkingHours(string open, string close, string start, string end, bool expected)
    {
        Assert.Equal(expected, PansionReserveRules.IsWithinWorkingHours(open, close, TimeSpan.Parse(start), TimeSpan.Parse(end)));
    }

    [Fact]
    public void DaycareHours_RoundUp()
    {
        Assert.Equal(2, PansionReserveRules.DaycareHourCount(TimeSpan.FromHours(8), TimeSpan.FromHours(9.5)));
    }

    [Fact]
    public void Reprice_LegacyDayPlusOneReserve_IsFixedToNights_KeepingRebate()
    {
        var pansion = new Pansion { PansionPrice = 800_000 };
        var reserve = new PansionReserve
        {
            FromDate = new DateTime(2026, 10, 12),
            ToDate = new DateTime(2026, 10, 13),
            DayCount = 2,
            Price = 1_600_000,
            RebatePrice = 100_000,
            PaymentPrice = 1_500_000
        };

        Assert.Equal(PansionReserveRules.RepriceResult.Changed, PansionReserveRules.Reprice(reserve, pansion));
        Assert.Equal(1, reserve.DayCount);
        Assert.Equal(800_000, reserve.Price);
        Assert.Equal(700_000, reserve.PaymentPrice);
        Assert.Equal(PansionReserveRules.RepriceResult.Unchanged, PansionReserveRules.Reprice(reserve, pansion));
    }

    [Fact]
    public void Reprice_SameDayStay_IsInvalid()
    {
        var reserve = new PansionReserve { FromDate = new DateTime(2026, 10, 12), ToDate = new DateTime(2026, 10, 12) };
        Assert.Equal(PansionReserveRules.RepriceResult.Invalid, PansionReserveRules.Reprice(reserve, new Pansion { PansionPrice = 1000 }));
    }

    [Fact]
    public void Reprice_Daycare_UsesHourlyPrice()
    {
        var reserve = new PansionReserve { StartTime = "08:00", EndTime = "10:30", Price = 1 };
        Assert.Equal(PansionReserveRules.RepriceResult.Changed, PansionReserveRules.Reprice(reserve, new Pansion { SchoolPrice = 50_000 }));
        Assert.Equal(3, reserve.HourCount);
        Assert.Equal(150_000, reserve.Price);
    }
}
