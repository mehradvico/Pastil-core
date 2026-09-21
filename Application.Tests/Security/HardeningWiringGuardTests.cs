using Xunit;

namespace Application.Tests.Security;

/// <summary>جلوی حذف تصادفی سیم‌کشی لاگ امنیتی، throttle ورود و هدرهای امنیتی را می‌گیرد.</summary>
public class HardeningWiringGuardTests
{
    private static string Root
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Pastil.sln")))
                directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("Pastil.sln not found.");
        }
    }

    private static string Read(string relative) => File.ReadAllText(Path.Combine(Root, relative.Replace('/', Path.DirectorySeparatorChar)));

    [Theory]
    [InlineData("Api/Program.cs")]
    [InlineData("File/Program.cs")]
    [InlineData("Payment/Program.cs")]
    public void Every_backend_service_sends_the_security_headers(string program)
    {
        Assert.Contains("app.UseBackendSecurityHeaders();", Read(program));
    }

    [Fact]
    public void Api_records_security_relevant_http_events_after_authentication()
    {
        var program = Read("Api/Program.cs");
        var auth = program.IndexOf("app.UseAuthentication();", StringComparison.Ordinal);
        var audit = program.IndexOf("app.UseMiddleware<Api.Middleware.SecurityAuditMiddleware>();", StringComparison.Ordinal);
        var authorization = program.IndexOf("app.UseAuthorization();", auth, StringComparison.Ordinal);

        Assert.True(auth >= 0 && audit > auth && audit < authorization,
            "SecurityAuditMiddleware must sit between UseAuthentication and UseAuthorization so it sees the user and the final 401/403/429.");
    }

    [Fact]
    public void Password_sign_in_and_change_password_use_the_per_account_throttle()
    {
        var source = Read("Application/Services/Accounting/UserSrv/UserService.cs");

        Assert.Contains("CheckPasswordThrottledAsync(item, user.Password, \"SignIn\"", source);
        Assert.Contains("_loginThrottle.RegisterFailure(\"pwd:\" + item.Id)", source);
        Assert.DoesNotContain("if (!await VerifyAndUpgradePasswordAsync(item, user.Password))", source);
    }

    [Fact]
    public void Api_has_a_global_rate_limiter_that_exempts_health_and_hubs()
    {
        var program = Read("Api/Program.cs");

        Assert.Contains("options.GlobalLimiter", program);
        Assert.Contains("StartsWithSegments(\"/health\")", program);
        Assert.Contains("StartsWithSegments(\"/hubs\")", program);
    }

    [Fact]
    public void Allowed_hosts_and_area_policy_mode_can_be_set_from_the_environment()
    {
        var secrets = Read("Application/Common/Configuration/SecretConfiguration.cs");

        Assert.Contains("\"AllowedHosts\", \"PASTIL_ALLOWED_HOSTS\"", secrets);
        Assert.Contains("\"Security:AreaPolicyMode\", \"PASTIL_AREA_POLICY_MODE\"", secrets);
        Assert.Contains("\"Security:OtpLength\", \"PASTIL_OTP_LENGTH\"", secrets);
    }

    [Fact]
    public void Legacy_RealTime_project_refuses_to_start_outside_development()
    {
        var program = Read("RealTime/Program.cs");

        Assert.Contains("if (!builder.Environment.IsDevelopment())", program);
        Assert.Contains("throw new InvalidOperationException(\"RealTime is a legacy project", program);
    }

    [Fact]
    public void Security_logs_are_exported_to_otlp_so_alerts_can_be_built_on_them()
    {
        var telemetry = Read("Utility/Observability/OpenTelemetryConfiguration.cs");

        Assert.Contains("\"Security.Audit\", LogLevel.Information", telemetry);
        Assert.Contains("AddOpenTelemetry(options =>", telemetry);
    }

    [Fact]
    public void Security_audit_feeds_the_realtime_alert_rules()
    {
        Assert.Contains("_alerts?.Observe(", Read("Application/Common/Security/SecurityAudit.cs"));
    }

    [Fact]
    public void Refresh_token_theft_detections_are_audited()
    {
        var source = Read("Application/Services/Accounting/UserTokenSrv/Srv/UserTokenService.cs");

        Assert.Contains("_audit.Failure(\"RefreshTokenTheft\"", source);
    }
}
