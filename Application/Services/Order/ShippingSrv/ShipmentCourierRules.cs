using Entities.Entities.ShippingField;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Application.Services.Order.ShippingSrv
{
    /// <summary>اطلاعات پیک که از یک وب‌هوک میاره بیرون کشیده می‌شود (هر فیلد ممکن است نیامده باشد).</summary>
    public sealed class CourierInfo
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public bool HasAnything =>
            !string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(Phone) || (Latitude.HasValue && Longitude.HasValue);
    }

    public enum TrackingAudience
    {
        Admin = 1,
        Seller = 2,
        Customer = 3
    }

    public sealed class TrackStep
    {
        public string Key { get; set; }
        public bool Done { get; set; }
        public bool Current { get; set; }
        public DateTime? AtUtc { get; set; }
    }

    // قواعد خالص «ردیابی پیک»: استخراج اطلاعات پیک از وب‌هوک میاره، پاک‌سازی متن خام، قواعد نمایش برای ادمین/فروشنده/مشتری،
    // مراحل و فاصله. طراحی: backend/Docs/MIARE_COURIER_TRACKING_FA.md
    //
    // ⚠️ ساختار دقیق وب‌هوک میاره در مخزن مستند نشده؛ استخراج عمداً تحمل‌پذیر است (نام‌های محتمل کلیدها، حساس‌نبودن به حروف)
    // و فقط از شیء پیک یا کلیدهای با پیشوند courier_ می‌خواند، تا شماره‌ی گیرنده/فرستنده به‌اشتباه «تلفن پیک» نشود.
    // متن خام هر رویداد در ShipmentEvents.PayloadJson ذخیره می‌شود تا بعد از اولین سفر واقعی نام فیلدها قطعی شود.
    public static class ShipmentCourierRules
    {
        public const int MaxPayloadChars = 6000;
        /// <summary>موقعیت‌های پشت‌سرهمِ یک state فقط هر این مدت یک ردیف رویداد می‌سازند (snapshot مرسوله همیشه به‌روز می‌شود).</summary>
        public static readonly TimeSpan MinSameStateEventGap = TimeSpan.FromSeconds(60);
        public const int MaxEventsPerShipment = 300;
        /// <summary>موقعیتی که قدیمی‌تر از این باشد «کهنه» است.</summary>
        public static readonly TimeSpan LocationStaleAfter = TimeSpan.FromMinutes(10);
        /// <summary>هر چند ثانیه اپ/وب‌اپ می‌تواند ردیابی را دوباره بخواند.</summary>
        public const int SuggestedPollSeconds = 15;

        private static readonly string[] CourierObjectKeys = { "courier", "driver", "rider", "delivery_man", "deliveryman", "agent", "assignee" };
        private static readonly string[] WrapperKeys = { "data", "trip", "payload", "event" };
        private static readonly string[] NameKeys = { "name", "full_name", "fullname", "display_name" };
        private static readonly string[] FirstNameKeys = { "first_name", "firstname" };
        private static readonly string[] LastNameKeys = { "last_name", "lastname" };
        private static readonly string[] PhoneKeys = { "phone", "phone_number", "mobile", "mobile_number", "cellphone", "tel" };
        private static readonly string[] LocationKeys = { "location", "current_location", "last_location", "position", "coordinates", "geo" };
        private static readonly string[] LatKeys = { "lat", "latitude" };
        private static readonly string[] LngKeys = { "lng", "lon", "long", "longitude" };
        private static readonly string[] CourierPrefixes = { "courier_", "driver_", "rider_" };

        // کلیدهایی که هرگز نباید در متن ذخیره‌شده بمانند (کد تحویل فقط به خود مشتری نشان داده می‌شود)
        private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "delivery_code", "deliverycode", "pod", "proof_of_delivery", "delivery_pin", "pin", "otp"
        };

        // ───────────────────────── استخراج از وب‌هوک ─────────────────────────

        public static CourierInfo Extract(JsonElement root)
        {
            var info = new CourierInfo();
            foreach (var candidate in Candidates(root))
            {
                Fill(info, candidate);
                if (info.HasAnything)
                    break;
            }
            return info;
        }

        private static IEnumerable<JsonElement> Candidates(JsonElement root)
        {
            if (root.ValueKind != JsonValueKind.Object)
                yield break;
            yield return root;
            foreach (var key in WrapperKeys)
                if (TryGet(root, key, out var inner) && inner.ValueKind == JsonValueKind.Object)
                    yield return inner;
        }

        private static void Fill(CourierInfo info, JsonElement scope)
        {
            // ۱) شیء پیک: کلیدها بدون پیشوند
            foreach (var key in CourierObjectKeys)
            {
                if (TryGet(scope, key, out var courier) && courier.ValueKind == JsonValueKind.Object)
                {
                    ReadInto(info, courier, string.Empty);
                    break;
                }
            }
            // ۲) کلیدهای تخت با پیشوند courier_ روی خود وب‌هوک (هرگز کلید بدون پیشوند: ممکن است مال گیرنده/فرستنده باشد)
            foreach (var prefix in CourierPrefixes)
                ReadInto(info, scope, prefix);
        }

        private static void ReadInto(CourierInfo info, JsonElement scope, string prefix)
        {
            if (string.IsNullOrEmpty(info.Name))
                info.Name = ReadName(scope, prefix);
            if (string.IsNullOrEmpty(info.Phone))
                info.Phone = NormalizePhone(ReadString(scope, PhoneKeys, prefix));
            if (!(info.Latitude.HasValue && info.Longitude.HasValue) && TryReadLocation(scope, prefix, out var lat, out var lng))
            {
                info.Latitude = lat;
                info.Longitude = lng;
            }
        }

        private static string ReadName(JsonElement scope, string prefix)
        {
            var full = CleanName(ReadString(scope, NameKeys, prefix));
            if (!string.IsNullOrEmpty(full))
                return full;
            var first = ReadString(scope, FirstNameKeys, prefix);
            var last = ReadString(scope, LastNameKeys, prefix);
            return CleanName($"{first} {last}");
        }

        private static bool TryReadLocation(JsonElement scope, string prefix, out double lat, out double lng)
        {
            foreach (var key in LocationKeys)
            {
                if (!TryGet(scope, prefix + key, out var location))
                    continue;
                if (location.ValueKind == JsonValueKind.Object && TryReadPair(location, string.Empty, out lat, out lng))
                    return true;
                if (location.ValueKind == JsonValueKind.Array && location.GetArrayLength() == 2)
                {
                    var items = location.EnumerateArray().ToArray();
                    if (TryReadNumber(items[0], out var first) && TryReadNumber(items[1], out var second)
                        && TryNormalizePair(first, second, out lat, out lng))
                        return true;
                }
            }
            return TryReadPair(scope, prefix, out lat, out lng);
        }

        private static bool TryReadPair(JsonElement scope, string prefix, out double lat, out double lng)
        {
            lat = lng = 0;
            double? latitude = null, longitude = null;
            foreach (var key in LatKeys)
                if (TryGet(scope, prefix + key, out var element) && TryReadNumber(element, out var number)) { latitude = number; break; }
            foreach (var key in LngKeys)
                if (TryGet(scope, prefix + key, out var element) && TryReadNumber(element, out var number)) { longitude = number; break; }
            if (!latitude.HasValue || !longitude.HasValue)
                return false;
            if (!IsUsable(latitude.Value, longitude.Value))
                return false;
            lat = latitude.Value;
            lng = longitude.Value;
            return true;
        }

        // آرایه‌ی دوتایی بدون نام: ترتیبِ [lat,lng] یا GeoJSON [lng,lat] — بازه‌ی ایران ترتیب را مشخص می‌کند
        public static bool TryNormalizePair(double first, double second, out double lat, out double lng)
        {
            lat = lng = 0;
            if (!IsUsable(first, second) && !IsUsable(second, first))
                return false;
            if (InIranLat(first) && InIranLng(second) && IsUsable(first, second)) { lat = first; lng = second; return true; }
            if (InIranLat(second) && InIranLng(first) && IsUsable(second, first)) { lat = second; lng = first; return true; }
            if (IsUsable(first, second)) { lat = first; lng = second; return true; }
            lat = second; lng = first;
            return true;
        }

        public static bool IsUsable(double lat, double lng) =>
            double.IsFinite(lat) && double.IsFinite(lng)
            && Math.Abs(lat) <= 90 && Math.Abs(lng) <= 180
            && !(Math.Abs(lat) < 1e-9 && Math.Abs(lng) < 1e-9);

        private static bool InIranLat(double value) => value >= 24 && value <= 40;
        private static bool InIranLng(double value) => value >= 43 && value <= 64;

        private static bool TryGet(JsonElement obj, string key, out JsonElement value)
        {
            if (obj.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in obj.EnumerateObject())
                {
                    if (string.Equals(property.Name, key, StringComparison.OrdinalIgnoreCase))
                    {
                        value = property.Value;
                        return true;
                    }
                }
            }
            value = default;
            return false;
        }

        private static string ReadString(JsonElement scope, string[] keys, string prefix)
        {
            foreach (var key in keys)
            {
                if (!TryGet(scope, prefix + key, out var element))
                    continue;
                if (element.ValueKind == JsonValueKind.String)
                    return element.GetString();
                if (element.ValueKind == JsonValueKind.Number)
                    return element.GetRawText();
            }
            return null;
        }

        private static bool TryReadNumber(JsonElement element, out double value)
        {
            value = 0;
            if (element.ValueKind == JsonValueKind.Number)
                return element.TryGetDouble(out value);
            if (element.ValueKind == JsonValueKind.String)
                return double.TryParse(element.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            return false;
        }

        public static string CleanName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            var cleaned = string.Join(' ', new string(value.Where(ch => !char.IsControl(ch)).ToArray())
                .Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
            if (cleaned.Length == 0)
                return null;
            return cleaned.Length > 200 ? cleaned[..200] : cleaned;
        }

        public static string NormalizePhone(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            var kept = new string(value.Where(ch => char.IsDigit(ch) || ch == '+').ToArray());
            return kept.Length is < 5 or > 40 ? null : kept;
        }

        /// <summary>اطلاعات تازه را روی snapshot مرسوله می‌نشاند: فیلدی که این‌بار نیامده، مقدار قبلی را پاک نمی‌کند. true = چیزی عوض شد.</summary>
        public static bool ApplyCourier(Shipment shipment, CourierInfo info, DateTime nowUtc)
        {
            var changed = false;
            if (!string.IsNullOrEmpty(info.Name) && shipment.CourierName != info.Name)
            {
                shipment.CourierName = info.Name;
                changed = true;
            }
            if (!string.IsNullOrEmpty(info.Phone) && shipment.CourierPhone != info.Phone)
            {
                shipment.CourierPhone = info.Phone;
                changed = true;
            }
            if (info.Latitude.HasValue && info.Longitude.HasValue)
            {
                shipment.CourierLatitude = info.Latitude;
                shipment.CourierLongitude = info.Longitude;
                shipment.CourierLocationAtUtc = nowUtc;
                changed = true;
            }
            return changed;
        }

        // ───────────────────────── متن خام و ذخیره‌ی رویداد ─────────────────────────

        /// <summary>متن خام وب‌هوک بدون کد تحویل/رمزها و کوتاه‌شده برای ذخیره؛ اگر JSON معتبر نباشد null.</summary>
        public static string SanitizePayload(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return null;
            JsonNode node;
            try
            {
                node = JsonNode.Parse(payload);
            }
            catch (JsonException)
            {
                return null;
            }
            if (node == null)
                return null;
            Strip(node);
            var text = node.ToJsonString();
            return text.Length > MaxPayloadChars ? text[..MaxPayloadChars] + "…" : text;
        }

        private static void Strip(JsonNode node)
        {
            if (node is JsonObject obj)
            {
                foreach (var key in obj.Select(pair => pair.Key).Where(SensitiveKeys.Contains).ToList())
                    obj.Remove(key);
                foreach (var pair in obj.ToList())
                    if (pair.Value != null)
                        Strip(pair.Value);
            }
            else if (node is JsonArray array)
            {
                foreach (var item in array)
                    if (item != null)
                        Strip(item);
            }
        }

        public static string HashPayload(string payload)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload ?? string.Empty));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        /// <summary>
        /// وب‌هوک میاره تا ۵ بار تکرار می‌شود و موقعیت پیک ممکن است هر چند ثانیه برسد؛ فقط رویدادِ تازه‌ی مفید ذخیره می‌شود:
        /// متن یکسان با آخرین رویداد = تکراری؛ state تازه = همیشه؛ همان state = حداکثر یک ردیف در هر MinSameStateEventGap؛
        /// بعد از MaxEventsPerShipment فقط تغییر state.
        /// </summary>
        public static bool ShouldStoreEvent(
            string lastState, DateTime? lastAtUtc, string lastHash,
            string newState, string newHash, DateTime nowUtc, int existingCount)
        {
            var stateChanged = !string.Equals(lastState, newState, StringComparison.OrdinalIgnoreCase);
            if (!stateChanged && lastHash != null && lastHash == newHash)
                return false;
            if (stateChanged)
                return true;
            if (existingCount >= MaxEventsPerShipment)
                return false;
            return !lastAtUtc.HasValue || nowUtc - lastAtUtc.Value >= MinSameStateEventGap;
        }

        // ───────────────────────── قواعد نمایش ─────────────────────────

        // نام: ادمین همیشه؛ فروشنده تا قبل از رسیدن پیک به فروشگاه (وضعیت ۳)؛ مشتری وقتی پیک در راه است (۳ و ۴)
        public static bool ShowName(TrackingAudience audience, ShipmentStatusEnum status) => audience switch
        {
            TrackingAudience.Admin => true,
            TrackingAudience.Seller => status == ShipmentStatusEnum.Accepted,
            _ => status is ShipmentStatusEnum.Accepted or ShipmentStatusEnum.PickedUp
        };

        // تلفن: ادمین همیشه؛ فروشنده فقط وقتی پیک به‌سمت فروشگاه می‌آید (هماهنگی تحویل)؛ مشتری فقط وقتی پیک کالا را برداشته و می‌آید
        public static bool ShowPhone(TrackingAudience audience, ShipmentStatusEnum status) => audience switch
        {
            TrackingAudience.Admin => true,
            TrackingAudience.Seller => status == ShipmentStatusEnum.Accepted,
            _ => status == ShipmentStatusEnum.PickedUp
        };

        // موقعیت زنده: ادمین همیشه (آخرین موقعیت معلوم)؛ فروشنده تا رسیدن پیک؛ مشتری وقتی پیک در راه است
        public static bool ShowLocation(TrackingAudience audience, ShipmentStatusEnum status) => audience switch
        {
            TrackingAudience.Admin => true,
            TrackingAudience.Seller => status == ShipmentStatusEnum.Accepted,
            _ => status is ShipmentStatusEnum.Accepted or ShipmentStatusEnum.PickedUp
        };

        public static bool IsLocationStale(DateTime? locationAtUtc, DateTime nowUtc) =>
            !locationAtUtc.HasValue || nowUtc - locationAtUtc.Value > LocationStaleAfter;

        public static double DistanceKm(double lat1, double lng1, double lat2, double lng2)
        {
            const double earthRadiusKm = 6371.0088;
            static double Rad(double degrees) => degrees * Math.PI / 180.0;
            var dLat = Rad(lat2 - lat1);
            var dLng = Rad(lng2 - lng1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                    + Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2)) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        // ───────────────────────── مراحل ─────────────────────────

        public static readonly string[] StepKeys = { "preparing", "courier_requested", "courier_assigned", "picked_up", "delivered" };

        /// <summary>کدام مرحله‌ی عادی بالاترین مرحله‌ی رسیده است (۰ تا ۴)؛ برای لغو/ناموفق از رویدادها و RequestedAt حدس زده می‌شود.</summary>
        public static int ReachedIndex(ShipmentStatusEnum status, IEnumerable<string> reachedStates, bool courierWasRequested)
        {
            var states = new HashSet<string>(reachedStates ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var fromStates = states.Contains("delivered") ? 4
                : states.Contains("dropoff") ? 3
                : states.Contains("pickup") ? 2
                : states.Contains("assign_queue") || courierWasRequested ? 1
                : 0;
            var fromStatus = status switch
            {
                ShipmentStatusEnum.Requested => 1,
                ShipmentStatusEnum.Accepted => 2,
                ShipmentStatusEnum.PickedUp => 3,
                ShipmentStatusEnum.Delivered => 4,
                _ => 0
            };
            return Math.Max(fromStates, fromStatus);
        }

        /// <summary>
        /// مراحل نمایشی. زمان هر مرحله را سرویس می‌گذارد. لغو/ناموفق: مراحل رسیده‌شده «انجام‌شده» می‌مانند و یک مرحله‌ی
        /// پایانی (cancelled / failed / returning) اضافه و «جاری» می‌شود.
        /// </summary>
        public static List<TrackStep> BuildSteps(ShipmentStatusEnum status, string failureReason, IEnumerable<string> reachedStates, bool courierWasRequested)
        {
            var reached = ReachedIndex(status, reachedStates, courierWasRequested);
            var steps = new List<TrackStep>();
            var negative = NegativeKey(status, failureReason);
            for (var index = 0; index < StepKeys.Length; index++)
            {
                var delivered = status == ShipmentStatusEnum.Delivered;
                steps.Add(new TrackStep
                {
                    Key = StepKeys[index],
                    Done = delivered || index < reached || (negative != null && index <= reached),
                    Current = negative == null && !delivered && index == reached
                });
            }
            if (negative != null)
                steps.Add(new TrackStep { Key = negative, Done = false, Current = true });
            return steps;
        }

        public static string NegativeKey(ShipmentStatusEnum status, string failureReason)
        {
            if (status == ShipmentStatusEnum.Cancelled)
                return "cancelled";
            if (status == ShipmentStatusEnum.Failed)
                return string.Equals(failureReason, "Returning", StringComparison.OrdinalIgnoreCase) ? "returning" : "failed";
            return null;
        }

        public static string CurrentKey(IReadOnlyList<TrackStep> steps) =>
            steps.FirstOrDefault(step => step.Current)?.Key ?? steps.LastOrDefault(step => step.Done)?.Key ?? StepKeys[0];
    }
}
