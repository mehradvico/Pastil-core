using Application.Services.CompanionSrvs.ConsultationOnlineSrv;
using System;
using Xunit;

namespace Application.Tests;

// «من آنلاینم» برای مشاوره: چراغ سبز فقط تا انقضا معتبر است، برای همیشه روشن نمی‌ماند.
public class ConsultationOnlineRulesTests
{
    private static readonly DateTime Now = new(2026, 9, 27, 2, 0, 0);

    [Fact]
    public void Flag_off_is_never_online_even_with_a_future_expiry()
        => Assert.False(ConsultationOnlineRules.IsOnline(false, Now.AddHours(1), Now));

    [Fact]
    public void Flag_on_without_expiry_is_not_online()
        => Assert.False(ConsultationOnlineRules.IsOnline(true, null, Now));

    [Fact]
    public void Flag_on_before_expiry_is_online()
        => Assert.True(ConsultationOnlineRules.IsOnline(true, Now.AddMinutes(5), Now));

    [Fact]
    public void Flag_on_after_expiry_is_offline()
        => Assert.False(ConsultationOnlineRules.IsOnline(true, Now.AddMinutes(-1), Now));

    [Fact]
    public void Expiry_is_three_hours_from_now()
        => Assert.Equal(Now.AddHours(3), ConsultationOnlineRules.ExpiresAt(Now));
}
