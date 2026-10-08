using Application.Services.Accounting.UserTokenSrv;
using Xunit;

namespace Application.Tests;

public class RefreshReuseRulesTests
{
    [Fact]
    public void A_token_revoked_by_another_login_has_no_successor_and_is_not_theft()
        // ورود دستگاه دوم همه‌ی توکن‌های دستگاه اول را Deleted می‌کند (بدون جانشین)؛ refresh بعدیِ دستگاه اول فقط رد می‌شود
        => Assert.Equal(RefreshReuseOutcome.SessionEnded, RefreshReuseRules.Classify(wasRotated: false, rotatedWithinGrace: false, deviceMatches: true));

    [Fact]
    public void Revoked_without_successor_never_escalates_even_if_the_device_differs()
        => Assert.Equal(RefreshReuseOutcome.SessionEnded, RefreshReuseRules.Classify(false, false, false));

    [Fact]
    public void A_just_rotated_token_reused_by_the_same_client_is_a_harmless_race()
        => Assert.Equal(RefreshReuseOutcome.Reissue, RefreshReuseRules.Classify(true, true, true));

    [Fact]
    public void A_rotated_token_reused_after_the_grace_period_is_theft()
        => Assert.Equal(RefreshReuseOutcome.TheftRevokeAll, RefreshReuseRules.Classify(true, false, true));

    [Fact]
    public void A_rotated_token_reused_from_another_device_is_theft()
        => Assert.Equal(RefreshReuseOutcome.TheftRevokeAll, RefreshReuseRules.Classify(true, true, false));
}
