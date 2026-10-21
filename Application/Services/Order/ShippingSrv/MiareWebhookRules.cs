using System;
using System.Text.Json;

namespace Application.Services.Order.ShippingSrv
{
    public static class MiareWebhookRules
    {
        public static bool TryReadTrip(JsonElement root, out string tripId, out string state, out JsonElement trip)
        {
            trip = root;
            tripId = null;
            state = null;
            if (root.ValueKind != JsonValueKind.Object)
                return false;

            // Miare v2 sends { "event": "assigned", "trip": { "id": ..., "state": ... } }.
            // Keep accepting a bare trip for previously configured integrations.
            if (root.TryGetProperty("trip", out var nested) && nested.ValueKind == JsonValueKind.Object)
                trip = nested;

            if (!trip.TryGetProperty("id", out var id) || id.ValueKind != JsonValueKind.String)
                return false;
            tripId = id.GetString();
            if (string.IsNullOrWhiteSpace(tripId))
                return false;

            if (trip.TryGetProperty("state", out var currentState) && currentState.ValueKind == JsonValueKind.String)
                state = currentState.GetString()?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(state) && root.TryGetProperty("event", out var eventName)
                && eventName.ValueKind == JsonValueKind.String)
            {
                state = eventName.GetString()?.Trim().ToLowerInvariant() switch
                {
                    "assigned" => "pickup",
                    "picked_up" => "dropoff",
                    "course_dropped_off" => "dropoff",
                    "delivered" => "delivered",
                    "returning" => "returning",
                    "canceled_by_client" => "canceled_by_client",
                    "canceled_by_miare" => "canceled_by_miare",
                    "canceled_by_delay" => "canceled_by_delay",
                    _ => null
                };
            }
            return !string.IsNullOrWhiteSpace(state);
        }
    }
}
