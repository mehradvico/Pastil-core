using Application.Services.CoachSrvs.CoachReserveSrv;
using Xunit;

namespace Application.Tests.Coach;

public class CoachReserveRulesTests
{
    [Theory]
    [InlineData("2026-10-17", 1)] // شنبه
    [InlineData("2026-10-18", 2)] // یکشنبه
    [InlineData("2026-10-19", 3)]
    [InlineData("2026-10-20", 4)]
    [InlineData("2026-10-21", 5)]
    [InlineData("2026-10-22", 6)]
    [InlineData("2026-10-23", 7)] // جمعه
    public void WeekDayNumber_MatchesWeekDayTable(string date, int expected) =>
        Assert.Equal(expected, CoachReserveRules.WeekDayNumber(DateTime.Parse(date)));

    [Theory]
    [InlineData(1_000_000, 200_000, 0, 1_000_000, 200_000)]
    [InlineData(1_000_000, 0, 0, 1_000_000, 1_000_000)]
    [InlineData(1_000_000, 1_000_000, 0, 1_000_000, 1_000_000)]
    [InlineData(1_000_000, 2_000_000, 0, 1_000_000, 1_000_000)] // پیش‌پرداخت بیشتر از قیمت نامعتبر است و کل قیمت می‌شود
    public void Amounts_Deposit(double package, double prepay, double pickup, double price, double online)
    {
        var amounts = CoachReserveRules.Amounts(new[] { (package, prepay) });
        Assert.Equal(price, amounts.Price);
        Assert.Equal(online, amounts.OnlineAmount);
    }

    [Theory]
    [InlineData(250_000, 0, 250_000)]
    [InlineData(250_000, 50_000, 200_000)]
    [InlineData(250_000, 250_000, 0)]
    [InlineData(250_000, 900_000, 0)] // تخفیف بیشتر از مبلغ: صفر، نه منفی
    [InlineData(250_000, -5, 250_000)] // تخفیف منفی نادیده گرفته می‌شود
    public void PaymentAfterRebate_NeverNegative(double online, double rebate, double expected) =>
        Assert.Equal(expected, CoachReserveRules.PaymentAfterRebate(online, rebate));

    [Theory]
    [InlineData(0, 12, 12)]
    [InlineData(8, 12, 8)]
    [InlineData(0, 0, 0)]
    public void ResolveCommission_ServiceWinsOtherwiseCoach(double service, double coach, double expected) =>
        Assert.Equal((decimal)expected, CoachReserveRules.ResolveCommission((decimal)service, (decimal)coach));

    [Theory]
    [InlineData(200_000, 10, 180_000, 20_000)]
    [InlineData(333_333, 10, 300_000, 33_333)] // سهم سایت گرد می‌شود و مجموع دقیقاً مبلغ است
    [InlineData(100_001, 10, 90_001, 10_000)]
    [InlineData(100_005, 10, 90_004, 10_001)] // 10000.5 گرد به بالا
    [InlineData(0, 10, 0, 0)]
    [InlineData(200_000, 0, 200_000, 0)]
    [InlineData(200_000, 100, 0, 200_000)]
    [InlineData(200_000, 150, 0, 200_000)] // درصد نامعتبر به ۱۰۰ بسته می‌شود
    [InlineData(200_000, -5, 200_000, 0)]
    public void Split_SumsExactly(double payment, double percent, double provider, double site)
    {
        var split = CoachReserveRules.Split(payment, (decimal)percent);
        Assert.Equal(provider, split.Provider);
        Assert.Equal(site, split.Site);
        Assert.Equal(payment, split.Provider + split.Site);
    }

    [Fact]
    public void Deadline_UsesWindowButNeverPastStartBufferOrBelowFloor()
    {
        var now = new DateTime(2026, 10, 19, 10, 0, 0);
        Assert.Equal(now.AddHours(12), CoachReserveRules.Deadline(now, now.AddDays(3)));
        Assert.Equal(now.AddHours(3).AddMinutes(-30), CoachReserveRules.Deadline(now, now.AddHours(3)));
        // نوبت خیلی نزدیک: حداقل ۳۰ دقیقه مهلت
        Assert.Equal(now.AddMinutes(30), CoachReserveRules.Deadline(now, now.AddMinutes(40)));
    }

    [Fact]
    public void MaxConcurrency_SweepsIntervals_AndTouchingIntervalsDoNotOverlap()
    {
        var day = new DateTime(2026, 10, 20);
        DateTime T(int h, int m = 0) => day.AddHours(h).AddMinutes(m);
        var existing = new[] { (T(9), T(10)), (T(10), T(11)), (T(9, 30), T(10, 30)) };

        // 9:30-10:30 با (9-10)، (10-11)، (9:30-10:30) هم‌پوشان است ولی هم‌زمانی لحظه‌ای حداکثر ۲ است
        Assert.Equal(2, CoachReserveRules.MaxConcurrency(existing, T(9, 30), T(10, 30)));
        // بازه‌ی بعد از همه‌ی رزروها خالی است
        Assert.Equal(0, CoachReserveRules.MaxConcurrency(existing, T(11), T(12)));
        // پایانِ یکی و شروعِ بعدی در یک لحظه = بدون هم‌پوشانی
        Assert.Equal(1, CoachReserveRules.MaxConcurrency(new[] { (T(9), T(10)), (T(10), T(11)) }, T(9), T(11)));
        Assert.Equal(0, CoachReserveRules.MaxConcurrency(Array.Empty<(DateTime, DateTime)>(), T(9), T(10)));
    }

    [Theory]
    [InlineData(3, 0, 3)]
    [InlineData(3, 2, 2)]
    [InlineData(1, 5, 1)]
    public void EffectiveCapacity_TakesTheSmallerLimit(int rowCapacity, int coachMax, int expected) =>
        Assert.Equal(expected, CoachReserveRules.EffectiveCapacity(rowCapacity, coachMax));

    [Fact]
    public void FreeCancel_IsTwelveHoursBeforeStart()
    {
        var now = new DateTime(2026, 10, 19, 10, 0, 0);
        Assert.True(CoachReserveRules.IsFreeCancel(now, now.AddHours(12)));
        Assert.False(CoachReserveRules.IsFreeCancel(now, now.AddHours(12).AddSeconds(-1)));
    }

    [Fact]
    public void Decisions_OnlyPaidActivePendingCanBeDecided()
    {
        Assert.True(CoachReserveRules.CanDecide(true, false, (int)CoachReserveOwnerDecisionEnum.Pending));
        Assert.False(CoachReserveRules.CanDecide(false, false, (int)CoachReserveOwnerDecisionEnum.Pending));
        Assert.False(CoachReserveRules.CanDecide(true, true, (int)CoachReserveOwnerDecisionEnum.Pending));
        Assert.False(CoachReserveRules.CanDecide(true, false, (int)CoachReserveOwnerDecisionEnum.Approved));
        Assert.True(CoachReserveRules.CanComplete((int)CoachReserveOwnerDecisionEnum.Approved));
        Assert.True(CoachReserveRules.CanComplete((int)CoachReserveOwnerDecisionEnum.NotRequired));
        Assert.False(CoachReserveRules.CanComplete((int)CoachReserveOwnerDecisionEnum.Pending));
        Assert.False(CoachReserveRules.CanComplete((int)CoachReserveOwnerDecisionEnum.Rejected));
        var now = DateTime.Now;
        Assert.True(CoachReserveRules.IsOverdue((int)CoachReserveOwnerDecisionEnum.Pending, now.AddMinutes(-1), now));
        Assert.False(CoachReserveRules.IsOverdue((int)CoachReserveOwnerDecisionEnum.Pending, now.AddMinutes(1), now));
        Assert.False(CoachReserveRules.IsOverdue((int)CoachReserveOwnerDecisionEnum.Approved, now.AddMinutes(-1), now));
    }

    [Theory]
    [InlineData("9:00", true)]
    [InlineData("09:00", true)]
    [InlineData("23:59", true)]
    [InlineData("24:00", false)]
    [InlineData("9", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void TryParseTime_AcceptsHourMinuteOnly(string? value, bool ok) =>
        Assert.Equal(ok, CoachReserveRules.TryParseTime(value!, out _));
}
