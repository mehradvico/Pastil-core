using Api.Hubs;
using Xunit;

namespace Application.Tests.Security;

public class CallSessionTrackerTests
{
    [Fact]
    public void A_connection_can_send_signals_only_for_the_call_it_joined()
    {
        var tracker = new CallSessionTracker();
        tracker.Join(reserveId: 101, connectionId: "connection-a", userId: 15);

        Assert.True(tracker.IsParticipant(101, "connection-a", 15));
        Assert.False(tracker.IsParticipant(102, "connection-a", 15));
        Assert.False(tracker.IsParticipant(101, "connection-a", 16));
        Assert.False(tracker.IsParticipant(101, "connection-b", 15));
    }

    [Fact]
    public void A_disconnected_connection_loses_signal_permission()
    {
        var tracker = new CallSessionTracker();
        tracker.Join(reserveId: 101, connectionId: "connection-a", userId: 15);

        tracker.Leave("connection-a");

        Assert.False(tracker.IsParticipant(101, "connection-a", 15));
    }
}
