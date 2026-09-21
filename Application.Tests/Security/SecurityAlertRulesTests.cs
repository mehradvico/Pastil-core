using Application.Common.Security;
using Xunit;

namespace Application.Tests.Security;

public class SecurityAlertRulesTests
{
    private sealed class Clock
    {
        public DateTime Utc { get; set; } = new(2026, 9, 20, 9, 0, 0, DateTimeKind.Utc); // ۱۲:۳۰ تهران
    }

    private static (SecurityAlertRules Rules, Clock Clock) Create()
    {
        var clock = new Clock();
        // تهران = UTC+3:30 (برای تست ثابت)
        return (new SecurityAlertRules(() => clock.Utc, utc => utc.AddHours(3.5)), clock);
    }

    private static SecurityEvent Fail(string name, string ip, long? user = null, string? detail = null) =>
        new() { Name = name, IsFailure = true, Ip = ip, UserId = user, Detail = detail };

    [Fact]
    public void Refresh_token_theft_alerts_immediately_and_is_deduplicated_per_user()
    {
        var (rules, clock) = Create();
        var theft = Fail("RefreshTokenTheft", "1.1.1.1", 42, "reuse_outside_grace");

        Assert.Single(rules.Evaluate(theft));
        Assert.Empty(rules.Evaluate(theft));

        clock.Utc = clock.Utc.AddMinutes(16);
        Assert.Single(rules.Evaluate(theft));
    }

    [Fact]
    public void Repeated_auth_failures_from_one_ip_alert_once_at_the_threshold()
    {
        var (rules, _) = Create();
        var alerts = new List<string>();

        for (var i = 0; i < 12; i++)
            alerts.AddRange(rules.Evaluate(Fail("SignIn", "5.5.5.5", 1, "password_invalid")));

        var alert = Assert.Single(alerts);
        Assert.Contains("5.5.5.5", alert);
    }

    [Fact]
    public void Failures_from_different_ips_do_not_add_up()
    {
        var (rules, _) = Create();
        var alerts = new List<string>();

        for (var i = 0; i < 30; i++)
            alerts.AddRange(rules.Evaluate(Fail("SignIn", $"9.9.9.{i}", i, "password_invalid")));

        Assert.Empty(alerts);
    }

    [Fact]
    public void Failures_spread_beyond_the_window_do_not_alert()
    {
        var (rules, clock) = Create();
        var alerts = new List<string>();

        for (var i = 0; i < 12; i++)
        {
            alerts.AddRange(rules.Evaluate(Fail("SignIn", "7.7.7.7", 1, "password_invalid")));
            clock.Utc = clock.Utc.AddMinutes(1);
        }

        Assert.Empty(alerts);
    }

    [Fact]
    public void Account_throttle_alerts_for_the_targeted_user()
    {
        var (rules, _) = Create();

        var alerts = rules.Evaluate(Fail("SignIn", "2.2.2.2", 77, "password_throttled"));

        Assert.Contains(alerts, message => message.Contains("77"));
    }

    [Fact]
    public void Admin_write_at_night_alerts_but_in_office_hours_does_not()
    {
        var (rules, clock) = Create();
        var write = new SecurityEvent { Name = "AdminWrite", IsFailure = false, UserId = 3, Ip = "3.3.3.3", Path = "/api/Admin/Product" };

        Assert.Empty(rules.Evaluate(write)); // ۱۲:۳۰ تهران

        clock.Utc = new DateTime(2026, 9, 20, 22, 0, 0, DateTimeKind.Utc); // ۰۱:۳۰ تهران
        Assert.Single(rules.Evaluate(write));
        Assert.Empty(rules.Evaluate(write)); // dedupe
    }

    [Fact]
    public void Panel_login_after_several_failures_for_the_same_user_alerts()
    {
        var (rules, _) = Create();
        for (var i = 0; i < 3; i++)
            rules.Evaluate(Fail("SignIn", "4.4.4.4", 88, "password_invalid"));

        var alerts = rules.Evaluate(new SecurityEvent { Name = "SignIn", IsFailure = false, UserId = 88, Ip = "4.4.4.4", Detail = "panel" });

        Assert.Single(alerts);
    }

    [Fact]
    public void Client_login_after_failures_and_unrelated_events_do_not_alert()
    {
        var (rules, _) = Create();
        for (var i = 0; i < 3; i++)
            rules.Evaluate(Fail("SignIn", "4.4.4.4", 88, "password_invalid"));

        Assert.Empty(rules.Evaluate(new SecurityEvent { Name = "SignIn", IsFailure = false, UserId = 88, Detail = "client" }));
        Assert.Empty(rules.Evaluate(new SecurityEvent { Name = "AreaNonMember", IsFailure = false, UserId = 88 }));
        Assert.Empty(rules.Evaluate(null!));
    }

    [Fact]
    public void Bursts_of_denied_requests_from_one_ip_alert()
    {
        var (rules, _) = Create();
        var alerts = new List<string>();

        for (var i = 0; i < 120; i++)
            alerts.AddRange(rules.Evaluate(Fail("Http403", "6.6.6.6")));

        Assert.Single(alerts);
    }
}
