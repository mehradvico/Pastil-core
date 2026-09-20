using Application.Common.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Application.Tests.Security;

public class LoginThrottleAndAuditTests
{
    private sealed class Clock
    {
        public DateTime Now { get; set; } = new(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc);
    }

    private static (LoginThrottle Throttle, Clock Clock) Create(int max = 5)
    {
        var clock = new Clock();
        return (new LoginThrottle(max, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15), () => clock.Now), clock);
    }

    [Fact]
    public void Account_is_blocked_after_the_configured_number_of_failures()
    {
        var (throttle, _) = Create();

        for (var i = 0; i < 4; i++)
        {
            throttle.RegisterFailure("pwd:1");
            Assert.False(throttle.IsBlocked("pwd:1", out _));
        }

        throttle.RegisterFailure("pwd:1");

        Assert.True(throttle.IsBlocked("pwd:1", out var retryAfter));
        Assert.InRange(retryAfter.TotalMinutes, 14, 15);
    }

    [Fact]
    public void Failures_of_one_account_never_block_another()
    {
        var (throttle, _) = Create(2);
        throttle.RegisterFailure("pwd:1");
        throttle.RegisterFailure("pwd:1");

        Assert.True(throttle.IsBlocked("pwd:1", out _));
        Assert.False(throttle.IsBlocked("pwd:2", out _));
    }

    [Fact]
    public void Block_expires_and_a_success_resets_the_counter()
    {
        var (throttle, clock) = Create(2);
        throttle.RegisterFailure("pwd:1");
        throttle.RegisterFailure("pwd:1");
        Assert.True(throttle.IsBlocked("pwd:1", out _));

        clock.Now = clock.Now.AddMinutes(16);
        Assert.False(throttle.IsBlocked("pwd:1", out _));

        throttle.RegisterFailure("pwd:1");
        throttle.Reset("pwd:1");
        throttle.RegisterFailure("pwd:1");
        Assert.False(throttle.IsBlocked("pwd:1", out _));
    }

    [Fact]
    public void Failures_outside_the_window_do_not_accumulate()
    {
        var (throttle, clock) = Create(3);
        throttle.RegisterFailure("pwd:1");
        throttle.RegisterFailure("pwd:1");

        clock.Now = clock.Now.AddMinutes(20);
        throttle.RegisterFailure("pwd:1");

        Assert.False(throttle.IsBlocked("pwd:1", out _));
    }

    [Theory]
    [InlineData("09121234567", "0912***4567")]
    [InlineData("vet@example.com", "v***@example.com")]
    [InlineData("12345", "***")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Identifiers_are_masked_before_they_reach_the_logs(string? value, string? expected)
    {
        Assert.Equal(expected, SecurityAudit.MaskIdentifier(value!));
    }

    private sealed class CapturingLogger : ILogger
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));
    }

    private sealed class CapturingFactory : ILoggerFactory
    {
        public CapturingLogger Logger { get; } = new();
        public string? Category { get; private set; }
        public void AddProvider(ILoggerProvider provider) { }
        public ILogger CreateLogger(string categoryName) { Category = categoryName; return Logger; }
        public void Dispose() { }
    }

    [Fact]
    public void Audit_events_use_the_dedicated_category_and_never_contain_the_raw_identifier()
    {
        var factory = new CapturingFactory();
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var audit = new SecurityAudit(factory, accessor);

        audit.Failure("SignIn", 42, "09121234567", "password_invalid");
        audit.Success("SignIn", 42, "09121234567");

        Assert.Equal("Security.Audit", factory.Category);
        Assert.Equal(LogLevel.Warning, factory.Logger.Entries[0].Level);
        Assert.Equal(LogLevel.Information, factory.Logger.Entries[1].Level);
        Assert.All(factory.Logger.Entries, entry =>
        {
            Assert.Contains("0912***4567", entry.Message);
            Assert.DoesNotContain("09121234567", entry.Message);
        });
    }

    [Fact]
    public void A_failing_logger_never_breaks_the_calling_flow()
    {
        var audit = new SecurityAudit(new ThrowingFactory(), new HttpContextAccessor());

        audit.Failure("SignIn");
    }

    private sealed class ThrowingFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider) { }
        public ILogger CreateLogger(string categoryName) => new ThrowingLogger();
        public void Dispose() { }
    }

    private sealed class ThrowingLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => throw new InvalidOperationException("sink down");
    }
}
