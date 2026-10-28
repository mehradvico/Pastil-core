using Application.Services.CommonSrv.CallIceSrv;
using System;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Application.Tests;

// iceServers تماس: بدون TURN مسیر رسانه پشت CGNAT ساخته نمی‌شود (هر طرف فقط دوربین خودش را می‌بیند).
public class CallIceRulesTests
{
    private static readonly DateTime Now = new(2026, 10, 28, 10, 0, 0, DateTimeKind.Utc);
    private static readonly string[] Turn = { "turn:t.example:3478?transport=udp", "turn:t.example:3478?transport=tcp" };

    [Fact]
    public void Empty_config_returns_no_servers()
        => Assert.Empty(CallIceRules.Build(new CallIceOptions(), 5, Now));

    [Fact]
    public void Turn_without_credentials_is_skipped()
    {
        var list = CallIceRules.Build(new CallIceOptions { StunUrls = new[] { "stun:t.example:3478" }, TurnUrls = Turn }, 5, Now);
        Assert.Single(list);
        Assert.Null(list[0].Username);
    }

    [Fact]
    public void Static_credential_is_passed_through()
    {
        var list = CallIceRules.Build(new CallIceOptions { TurnUrls = Turn, TurnUsername = "u", TurnCredential = "c" }, 5, Now);
        Assert.Equal(("u", "c"), (list[0].Username, list[0].Credential));
        Assert.Equal(2, list[0].Urls.Length);
    }

    [Fact]
    public void Shared_secret_yields_coturn_rest_credential()
    {
        var list = CallIceRules.Build(new CallIceOptions { TurnUrls = Turn, TurnSharedSecret = "s", TurnUsername = "u", TurnCredential = "c", CredentialTtlMinutes = 60 }, 7, Now);
        var expiry = new DateTimeOffset(Now).AddMinutes(60).ToUnixTimeSeconds();
        Assert.Equal($"{expiry}:7", list[0].Username);
        using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes("s"));
        Assert.Equal(Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(list[0].Username))), list[0].Credential);
    }
}
