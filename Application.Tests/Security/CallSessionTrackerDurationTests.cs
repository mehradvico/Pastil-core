using Api.Hubs;
using Xunit;

namespace Application.Tests.Security;

// اندازه‌گیری مدت واقعی تماس جلسه‌ی مشاوره: هر «قطعه‌ی تماس» فقط یک‌بار شمرده می‌شود.
public class CallSessionTrackerDurationTests
{
    [Fact]
    public void Marking_connected_twice_starts_only_one_segment()
    {
        var tracker = new CallSessionTracker();
        Assert.True(tracker.MarkConnected(-5));
        Assert.False(tracker.MarkConnected(-5));
    }

    [Fact]
    public void Taking_seconds_clears_the_segment_so_a_second_leave_adds_nothing()
    {
        var tracker = new CallSessionTracker();
        tracker.MarkConnected(-5);

        Assert.True(tracker.TakeConnectedSeconds(-5) >= 0);
        // نفر دوم هم خارج می‌شود: قطعه‌ی تماس قبلاً بسته شده و چیزی اضافه نمی‌شود
        Assert.Equal(0, tracker.TakeConnectedSeconds(-5));
    }

    [Fact]
    public void A_reconnect_after_a_leave_starts_a_new_segment()
    {
        var tracker = new CallSessionTracker();
        tracker.MarkConnected(-9);
        tracker.TakeConnectedSeconds(-9);

        Assert.True(tracker.MarkConnected(-9));
    }

    [Fact]
    public void Segments_of_different_sessions_do_not_interfere()
    {
        var tracker = new CallSessionTracker();
        tracker.MarkConnected(-1);
        tracker.MarkConnected(-2);

        tracker.TakeConnectedSeconds(-1);
        Assert.False(tracker.MarkConnected(-2));
    }

    [Fact]
    public void Taking_seconds_of_a_call_that_never_connected_is_zero()
    {
        Assert.Equal(0, new CallSessionTracker().TakeConnectedSeconds(-77));
    }
}
