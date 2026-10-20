using Application.Services.Order.ShippingSrv;
using Entities.Entities.ShippingField;
using System.Text.Json;
using Xunit;

namespace Application.Tests;

public class ShipmentCourierRulesTests
{
    private static CourierInfo Extract(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return ShipmentCourierRules.Extract(doc.RootElement);
    }

    [Fact]
    public void Extract_ReadsNestedCourierObject_WithLocationObject()
    {
        var info = Extract("""
            { "id": "t1", "state": "pickup",
              "courier": { "name": "علی  رضایی", "phone": "0912 345 6789",
                           "location": { "lat": 35.7, "lng": 51.4 } } }
            """);

        Assert.Equal("علی رضایی", info.Name);
        Assert.Equal("09123456789", info.Phone);
        Assert.Equal(35.7, info.Latitude);
        Assert.Equal(51.4, info.Longitude);
    }

    [Fact]
    public void Extract_ReadsFlatCourierPrefixedKeys_AndFirstLastName()
    {
        var info = Extract("""
            { "id": "t1", "courier_first_name": "سارا", "courier_last_name": "کریمی",
              "courier_mobile": "09351112233", "courier_latitude": "35.71", "courier_longitude": "51.41" }
            """);

        Assert.Equal("سارا کریمی", info.Name);
        Assert.Equal("09351112233", info.Phone);
        Assert.Equal(35.71, info.Latitude);
        Assert.Equal(51.41, info.Longitude);
    }

    [Fact]
    public void Extract_NeverReadsUnprefixedRootKeys_ThatBelongToTheRecipientOrSender()
    {
        var info = Extract("""
            { "id": "t1", "name": "گیرنده", "phone": "09120000000", "location": { "lat": 35.7, "lng": 51.4 },
              "dropoff": { "phone": "09121111111" } }
            """);

        Assert.False(info.HasAnything);
    }

    [Fact]
    public void Extract_LooksInsideDataWrapper()
    {
        var info = Extract("""{ "event": "x", "data": { "driver": { "full_name": "رضا", "mobile": "09124445566" } } }""");

        Assert.Equal("رضا", info.Name);
        Assert.Equal("09124445566", info.Phone);
    }

    [Theory]
    [InlineData("[51.4, 35.7]")]   // GeoJSON: lng, lat
    [InlineData("[35.7, 51.4]")]   // lat, lng
    public void Extract_ArrayLocation_IsOrderedByIranRange(string array)
    {
        var info = Extract($$"""{ "courier": { "name": "a", "location": {{array}} } }""");

        Assert.Equal(35.7, info.Latitude);
        Assert.Equal(51.4, info.Longitude);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(95, 51)]
    [InlineData(35, 200)]
    public void IsUsable_RejectsZeroAndOutOfRange(double lat, double lng) =>
        Assert.False(ShipmentCourierRules.IsUsable(lat, lng));

    [Fact]
    public void Extract_IgnoresUnusableCoordinates()
    {
        var info = Extract("""{ "courier": { "name": "a", "lat": 0, "lng": 0 } }""");

        Assert.Null(info.Latitude);
        Assert.Equal("a", info.Name);
    }

    [Theory]
    [InlineData("abc", null)]
    [InlineData("1234", null)]
    [InlineData("+98 912 345 6789", "+989123456789")]
    [InlineData(null, null)]
    public void NormalizePhone_KeepsDigitsAndPlus(string value, string expected) =>
        Assert.Equal(expected, ShipmentCourierRules.NormalizePhone(value));

    [Fact]
    public void SanitizePayload_RemovesDeliveryCodeEverywhere_ButKeepsTheRest()
    {
        var text = ShipmentCourierRules.SanitizePayload("""
            { "id": "t1", "state": "dropoff", "delivery_code": "12345",
              "dropoff": { "Delivery_Code": "12345", "address": "x" },
              "items": [ { "pod": "99", "keep": 1 } ] }
            """);

        Assert.DoesNotContain("12345", text);
        Assert.DoesNotContain("\"pod\"", text);
        Assert.Contains("\"state\":\"dropoff\"", text);
        Assert.Contains("\"keep\":1", text);
    }

    [Fact]
    public void SanitizePayload_RejectsInvalidJson_AndTruncatesHugeOnes()
    {
        Assert.Null(ShipmentCourierRules.SanitizePayload("not json"));
        Assert.Null(ShipmentCourierRules.SanitizePayload(" "));

        var huge = "{\"a\":\"" + new string('x', 20000) + "\"}";
        var text = ShipmentCourierRules.SanitizePayload(huge);
        Assert.True(text.Length <= ShipmentCourierRules.MaxPayloadChars + 1);
    }

    [Fact]
    public void HashPayload_IsStableAndDifferentForDifferentPayloads()
    {
        Assert.Equal(ShipmentCourierRules.HashPayload("a"), ShipmentCourierRules.HashPayload("a"));
        Assert.NotEqual(ShipmentCourierRules.HashPayload("a"), ShipmentCourierRules.HashPayload("b"));
        Assert.Equal(64, ShipmentCourierRules.HashPayload("a").Length);
    }

    [Fact]
    public void ShouldStoreEvent_DropsRetries_KeepsStateChanges_ThrottlesSameStateLocations()
    {
        var now = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

        // retry تکراری میاره (همان state، همان متن)
        Assert.False(ShipmentCourierRules.ShouldStoreEvent("pickup", now.AddSeconds(-5), "h1", "pickup", "h1", now, 3));
        // state تازه همیشه ذخیره می‌شود
        Assert.True(ShipmentCourierRules.ShouldStoreEvent("pickup", now.AddSeconds(-1), "h1", "dropoff", "h2", now, 3));
        // همان state با موقعیت تازه: فقط بعد از یک دقیقه
        Assert.False(ShipmentCourierRules.ShouldStoreEvent("dropoff", now.AddSeconds(-20), "h1", "dropoff", "h2", now, 3));
        Assert.True(ShipmentCourierRules.ShouldStoreEvent("dropoff", now.AddSeconds(-61), "h1", "dropoff", "h2", now, 3));
        // سقف تعداد: فقط تغییر state
        Assert.False(ShipmentCourierRules.ShouldStoreEvent("dropoff", now.AddMinutes(-5), "h1", "dropoff", "h2", now, ShipmentCourierRules.MaxEventsPerShipment));
        Assert.True(ShipmentCourierRules.ShouldStoreEvent("dropoff", now.AddMinutes(-5), "h1", "delivered", "h3", now, ShipmentCourierRules.MaxEventsPerShipment));
        // اولین رویداد
        Assert.True(ShipmentCourierRules.ShouldStoreEvent(null, null, null, "assign_queue", "h1", now, 0));
    }

    [Fact]
    public void ApplyCourier_NeverBlanksOldValues_AndStampsLocationTime()
    {
        var shipment = new Shipment { CourierName = "قبلی", CourierPhone = "09120000001" };
        var now = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);

        // این بار فقط موقعیت آمده
        var changed = ShipmentCourierRules.ApplyCourier(shipment, new CourierInfo { Latitude = 35.7, Longitude = 51.4 }, now);

        Assert.True(changed);
        Assert.Equal("قبلی", shipment.CourierName);
        Assert.Equal("09120000001", shipment.CourierPhone);
        Assert.Equal(now, shipment.CourierLocationAtUtc);

        // رویداد بدون هیچ اطلاعات پیک چیزی را عوض نمی‌کند
        Assert.False(ShipmentCourierRules.ApplyCourier(shipment, new CourierInfo(), now.AddMinutes(1)));
        Assert.Equal(now, shipment.CourierLocationAtUtc);
    }

    [Theory]
    // مشتری: نام و موقعیت وقتی پیک در راه است؛ تلفن فقط بعد از برداشتن کالا؛ هرگز بعد از پایان
    [InlineData(TrackingAudience.Customer, ShipmentStatusEnum.Requested, false, false, false)]
    [InlineData(TrackingAudience.Customer, ShipmentStatusEnum.Accepted, true, false, true)]
    [InlineData(TrackingAudience.Customer, ShipmentStatusEnum.PickedUp, true, true, true)]
    [InlineData(TrackingAudience.Customer, ShipmentStatusEnum.Delivered, false, false, false)]
    [InlineData(TrackingAudience.Customer, ShipmentStatusEnum.Cancelled, false, false, false)]
    // فروشنده: فقط وقتی پیک به‌سمت فروشگاه می‌آید
    [InlineData(TrackingAudience.Seller, ShipmentStatusEnum.Requested, false, false, false)]
    [InlineData(TrackingAudience.Seller, ShipmentStatusEnum.Accepted, true, true, true)]
    [InlineData(TrackingAudience.Seller, ShipmentStatusEnum.PickedUp, false, false, false)]
    [InlineData(TrackingAudience.Seller, ShipmentStatusEnum.Delivered, false, false, false)]
    // ادمین: همیشه
    [InlineData(TrackingAudience.Admin, ShipmentStatusEnum.Delivered, true, true, true)]
    [InlineData(TrackingAudience.Admin, ShipmentStatusEnum.Failed, true, true, true)]
    public void VisibilityMatrix(TrackingAudience audience, ShipmentStatusEnum status, bool name, bool phone, bool location)
    {
        Assert.Equal(name, ShipmentCourierRules.ShowName(audience, status));
        Assert.Equal(phone, ShipmentCourierRules.ShowPhone(audience, status));
        Assert.Equal(location, ShipmentCourierRules.ShowLocation(audience, status));
    }

    [Fact]
    public void LocationStale_AfterTenMinutes()
    {
        var now = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);
        Assert.False(ShipmentCourierRules.IsLocationStale(now.AddMinutes(-9), now));
        Assert.True(ShipmentCourierRules.IsLocationStale(now.AddMinutes(-11), now));
        Assert.True(ShipmentCourierRules.IsLocationStale(null, now));
    }

    [Fact]
    public void DistanceKm_TehranToKaraj_IsAboutForty()
    {
        var km = ShipmentCourierRules.DistanceKm(35.6892, 51.3890, 35.8355, 50.9915);
        Assert.InRange(km, 35, 45);
        Assert.Equal(0, ShipmentCourierRules.DistanceKm(35.7, 51.4, 35.7, 51.4), 6);
    }

    private static string[] Keys(List<TrackStep> steps) => steps.Select(step => step.Key).ToArray();

    [Theory]
    [InlineData(ShipmentStatusEnum.AwaitingSellerConfirm, "preparing")]
    [InlineData(ShipmentStatusEnum.Preparing, "preparing")]
    [InlineData(ShipmentStatusEnum.Requested, "courier_requested")]
    [InlineData(ShipmentStatusEnum.Accepted, "courier_assigned")]
    [InlineData(ShipmentStatusEnum.PickedUp, "picked_up")]
    public void BuildSteps_MarksEarlierDone_AndTheCurrentOne(ShipmentStatusEnum status, string current)
    {
        var steps = ShipmentCourierRules.BuildSteps(status, null, Array.Empty<string>(), courierWasRequested: false);

        Assert.Equal(5, steps.Count);
        Assert.Equal(current, ShipmentCourierRules.CurrentKey(steps));
        var currentIndex = steps.FindIndex(step => step.Current);
        Assert.All(steps.Take(currentIndex), step => Assert.True(step.Done));
        Assert.All(steps.Skip(currentIndex + 1), step => Assert.False(step.Done));
    }

    [Fact]
    public void BuildSteps_Delivered_AllDoneNoneCurrent()
    {
        var steps = ShipmentCourierRules.BuildSteps(ShipmentStatusEnum.Delivered, null, Array.Empty<string>(), true);

        Assert.All(steps, step => Assert.True(step.Done));
        Assert.DoesNotContain(steps, step => step.Current);
        Assert.Equal("delivered", ShipmentCourierRules.CurrentKey(steps));
    }

    [Fact]
    public void BuildSteps_CancelledAfterPickup_KeepsReachedStepsDone_AndAddsCancelledStep()
    {
        var steps = ShipmentCourierRules.BuildSteps(ShipmentStatusEnum.Cancelled, null, new[] { "assign_queue", "pickup" }, true);

        Assert.Equal(new[] { "preparing", "courier_requested", "courier_assigned", "picked_up", "delivered", "cancelled" }, Keys(steps));
        Assert.True(steps[2].Done);
        Assert.False(steps[3].Done);
        Assert.Equal("cancelled", ShipmentCourierRules.CurrentKey(steps));
    }

    [Theory]
    [InlineData("Returning", "returning")]
    [InlineData("Miare state: canceled_by_delay", "failed")]
    [InlineData(null, "failed")]
    public void BuildSteps_Failed_UsesReturningOrFailedStep(string reason, string expected)
    {
        var steps = ShipmentCourierRules.BuildSteps(ShipmentStatusEnum.Failed, reason, new[] { "dropoff" }, true);

        Assert.Equal(expected, steps[^1].Key);
        Assert.True(steps[^1].Current);
        Assert.True(steps[3].Done); // به «در راه» رسیده بود
    }
}
