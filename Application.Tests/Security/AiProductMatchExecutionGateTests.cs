using Api.Services.AiProductMatch;
using Xunit;

namespace Application.Tests.Security;

public class AiProductMatchExecutionGateTests
{
    [Fact]
    public void Gate_allows_only_four_concurrent_analyses_and_releases_a_slot_once()
    {
        var gate = new AiProductMatchExecutionGate();
        var leases = Enumerable.Range(0, 4).Select(_ => gate.TryAcquire()).ToArray();

        Assert.All(leases, lease => Assert.NotNull(lease));
        Assert.Null(gate.TryAcquire());

        leases[0]!.Dispose();
        leases[0]!.Dispose();

        Assert.NotNull(gate.TryAcquire());
    }
}
