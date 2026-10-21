using Application.Services.Order.ShippingSrv;
using System.Text.Json;
using Xunit;

namespace Application.Tests;

public class MiareWebhookRulesTests
{
    [Theory]
    [InlineData("assigned", "pickup", "pickup")]
    [InlineData("picked_up", "dropoff", "dropoff")]
    [InlineData("delivered", "delivered", "delivered")]
    public void Reads_current_trip_state_from_official_envelope(string eventName, string tripState, string expected)
    {
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            @event = eventName,
            trip = new { id = "trip-123", state = tripState, courier = new { name = "Ali" } }
        }));

        Assert.True(MiareWebhookRules.TryReadTrip(doc.RootElement, out var id, out var state, out var trip));
        Assert.Equal("trip-123", id);
        Assert.Equal(expected, state);
        Assert.Equal("Ali", trip.GetProperty("courier").GetProperty("name").GetString());
    }

    [Fact]
    public void Accepts_legacy_bare_trip()
    {
        using var doc = JsonDocument.Parse("""{"id":"trip-123","state":"delivered"}""");
        Assert.True(MiareWebhookRules.TryReadTrip(doc.RootElement, out var id, out var state, out _));
        Assert.Equal("trip-123", id);
        Assert.Equal("delivered", state);
    }

    [Fact]
    public void Uses_documented_event_when_state_is_missing()
    {
        using var doc = JsonDocument.Parse("""{"event":"assigned","trip":{"id":"trip-123"}}""");
        Assert.True(MiareWebhookRules.TryReadTrip(doc.RootElement, out _, out var state, out _));
        Assert.Equal("pickup", state);
    }

    [Fact]
    public void Does_not_mistake_event_or_course_id_for_trip_id()
    {
        using var doc = JsonDocument.Parse("""{"event":"course_added__course","course":{"id":"course-123","trip_id":"trip-123"}}""");
        Assert.False(MiareWebhookRules.TryReadTrip(doc.RootElement, out _, out _, out _));
    }
}
