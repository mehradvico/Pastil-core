using Application.Services.PastilAISrv.Provider;
using Xunit;

namespace Application.Tests;

public class PastilAiProviderCooldownTests
{
    [Theory]
    [InlineData(401, 15)]
    [InlineData(402, 15)]
    [InlineData(403, 15)]
    [InlineData(404, 15)]
    [InlineData(429, 5)]
    public void Persistent_http_failures_cool_the_provider_down(int status, int minutes)
    {
        Assert.Equal(TimeSpan.FromMinutes(minutes), PastilAiProviderCooldown.Compute(status, "http_" + status));
    }

    [Fact]
    public void Timeouts_cool_down_briefly_and_transient_errors_do_not()
    {
        Assert.Equal(TimeSpan.FromMinutes(2), PastilAiProviderCooldown.Compute(null, "timeout"));
        Assert.Null(PastilAiProviderCooldown.Compute(500, "http_500"));
        Assert.Null(PastilAiProviderCooldown.Compute(200, null));
    }

    [Fact]
    public void A_failed_provider_is_skipped_until_it_succeeds_again()
    {
        const string name = "UnitTestProvider";
        PastilAiProviderCooldown.MarkFailed(name, 402, "http_402");
        Assert.True(PastilAiProviderCooldown.IsCoolingDown(name));

        PastilAiProviderCooldown.MarkSucceeded(name);
        Assert.False(PastilAiProviderCooldown.IsCoolingDown(name));
    }
}
