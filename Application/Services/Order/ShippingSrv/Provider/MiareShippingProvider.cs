using Entities.Entities.ShippingField;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv.Provider
{
    // میاره (Miare) سرویس واقعی پیک/ارسال است — برخلاف سایر ارائه‌دهنده‌ها (AloPeyk/Tipax/SnappBox)
    // که هنوز فقط حالت تست دارند، این کلاس مستقیماً با API واقعی میاره صحبت می‌کند.
    // GetQuoteAsync از endpoint استعلام قیمت میاره (روی Base URL جدای Accounting) استفاده می‌کند؛
    // این فقط یک برآورد است، نه مبلغ نهایی. مبلغ نهایی واقعی («delivery_cost») بعد از تحویل
    // از طریق Webhook میاره (MiareWebhookController) دریافت می‌شود.
    public class MiareShippingProvider : IShippingProvider, IBoostCapableShippingProvider
    {
        // سطوح boost کم‌تغییرند (مستند میاره: «به‌ندرت عوض می‌شوند؛ دوره‌ای بگیرید»)؛ بین درخواست‌ها چند دقیقه کش می‌شود
        private static readonly object BoostCacheLock = new();
        private static IReadOnlyList<ShippingBoostLevel> _boostCache;
        private static DateTime _boostCacheExpiresUtc = DateTime.MinValue;
        private static readonly TimeSpan BoostCacheTtl = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan BoostFailureCacheTtl = TimeSpan.FromMinutes(2);

        private readonly ShippingOptions _options;
        private readonly ILogger<MiareShippingProvider> _logger;

        public MiareShippingProvider(IOptions<ShippingOptions> options, ILogger<MiareShippingProvider> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public ShippingProviderEnum Provider => ShippingProviderEnum.Miare;

        public async Task<ShippingProviderQuoteResult> GetQuoteAsync(
            ShippingProviderQuoteRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var providerOptions = _options.Miare;
            if (!providerOptions.Enabled)
            {
                _logger.LogWarning("Miare GetQuoteAsync: provider disabled (Shipping:Miare:Enabled=false).");
                return ShippingProviderQuoteResult.Failed(Resource.Notification.ShippingProviderServiceDisabled);
            }

            if (string.IsNullOrWhiteSpace(providerOptions.AccountingBaseUrl) || string.IsNullOrWhiteSpace(providerOptions.ApiKey))
            {
                _logger.LogWarning(
                    "Miare GetQuoteAsync: connection settings incomplete (AccountingBaseUrl empty: {AccountingUrlEmpty}, ApiKey empty: {ApiKeyEmpty}).",
                    string.IsNullOrWhiteSpace(providerOptions.AccountingBaseUrl),
                    string.IsNullOrWhiteSpace(providerOptions.ApiKey));
                return ShippingProviderQuoteResult.Failed(
                    string.Format(Resource.Notification.ShippingProviderConnectionSettingsIncompleteFormat, Provider));
            }

            try
            {
                var client = new RestClient(new RestClientOptions(providerOptions.AccountingBaseUrl.TrimEnd('/')));
                var restRequest = new RestRequest("/estimate/price/", Method.Get);
                restRequest.AddHeader("Authorization", $"Token {providerOptions.ApiKey}");
                restRequest.AddQueryParameter("source", $"{request.OriginLatitude},{request.OriginLongitude}");
                restRequest.AddQueryParameter("destination", $"{request.DestinationLatitude},{request.DestinationLongitude}");

                var response = await client.ExecuteAsync(restRequest, cancellationToken);
                _logger.LogInformation(
                    "Miare GetQuoteAsync: GET {Url} -> {StatusCode}, body: {Body}",
                    $"{providerOptions.AccountingBaseUrl.TrimEnd('/')}/estimate/price/?source={request.OriginLatitude},{request.OriginLongitude}&destination={request.DestinationLatitude},{request.DestinationLongitude}",
                    (int)response.StatusCode,
                    response.Content);

                if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
                {
                    var error = ExtractMiareError(response.Content) ?? response.ErrorMessage ?? "Miare estimate request failed.";
                    _logger.LogWarning(response.ErrorException, "Miare GetQuoteAsync: request failed - {Error}", error);
                    return ShippingProviderQuoteResult.Failed(error);
                }

                using var doc = JsonDocument.Parse(response.Content);
                var root = doc.RootElement;

                var areaCovered = !root.TryGetProperty("area_coverage", out var coverageProp) ||
                    coverageProp.ValueKind != JsonValueKind.False;
                if (!areaCovered)
                {
                    _logger.LogWarning("Miare GetQuoteAsync: area_coverage=false for this address.");
                    return ShippingProviderQuoteResult.Failed(
                        string.Format(Resource.Notification.ShippingProviderAreaNotCoveredFormat, Provider));
                }

                if (!root.TryGetProperty("price", out var priceProp) || priceProp.ValueKind != JsonValueKind.Number)
                {
                    _logger.LogWarning("Miare GetQuoteAsync: response did not include a numeric price.");
                    return ShippingProviderQuoteResult.Failed("Miare estimate response did not include a price.");
                }

                // عدد استعلام میاره همان مبلغ تومانی مشتری است؛ تنظیم قدیمی واحد نباید آن را تقسیم کند.
                var rawPrice = priceProp.GetDouble();
                var priceToman = MiareAmountRules.ToToman(rawPrice, providerOptions.AmountUnit);
                // میاره هیچ‌وقت ارسال رایگان نیست؛ قیمت صفر یعنی برآورد معتبر نیست (مثلاً محیط تست/آدرس نامعتبر) و به‌جای «رایگان» استعلام شکست می‌خورد.
                if (priceToman <= 0)
                {
                    _logger.LogWarning("Miare GetQuoteAsync: estimate returned a non-positive price ({Price}).", priceToman);
                    return ShippingProviderQuoteResult.Failed(Resource.Notification.ShippingQuoteCurrentlyUnavailable);
                }
                return new ShippingProviderQuoteResult
                {
                    IsSuccess = true,
                    Price = priceToman,
                    Currency = "IRT",
                    ExternalQuoteId = $"MIARE-EST-{Guid.NewGuid():N}"
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Miare GetQuoteAsync: unhandled exception.");
                return ShippingProviderQuoteResult.Failed(exception.Message);
            }
        }

        // pickup.deadline: UTC با «Z». میاره آفست با دونقطه («+00:00») را با parse_error رد می‌کند (تست staging)
        public static string FormatDeadline(DateTime utc) =>
            DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", System.Globalization.CultureInfo.InvariantCulture);

        public async Task<IReadOnlyList<ShippingBoostLevel>> GetBoostLevelsAsync(CancellationToken cancellationToken = default)
        {
            lock (BoostCacheLock)
            {
                if (_boostCache != null && DateTime.UtcNow < _boostCacheExpiresUtc)
                    return _boostCache;
            }

            IReadOnlyList<ShippingBoostLevel> levels = Array.Empty<ShippingBoostLevel>();
            var ttl = BoostFailureCacheTtl;
            var providerOptions = _options.Miare;
            if (providerOptions.Enabled &&
                !string.IsNullOrWhiteSpace(providerOptions.BaseUrl) && !string.IsNullOrWhiteSpace(providerOptions.ApiKey))
            {
                try
                {
                    var client = new RestClient(new RestClientOptions(providerOptions.BaseUrl.TrimEnd('/')));
                    var request = new RestRequest("/boost-levels/", Method.Get);
                    request.AddHeader("Authorization", $"Token {providerOptions.ApiKey}");
                    var response = await client.ExecuteAsync(request, cancellationToken);
                    if (response.IsSuccessful && !string.IsNullOrWhiteSpace(response.Content))
                    {
                        using var doc = JsonDocument.Parse(response.Content);
                        if (doc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            levels = doc.RootElement.EnumerateArray()
                                .Where(item => item.TryGetProperty("level", out var level) && level.ValueKind == JsonValueKind.Number
                                               && item.TryGetProperty("amount", out var amount) && amount.ValueKind == JsonValueKind.Number)
                                .Select(item => new ShippingBoostLevel { Level = item.GetProperty("level").GetInt32(), Amount = MiareAmountRules.ToToman(item.GetProperty("amount").GetDouble(), providerOptions.AmountUnit) })
                                .OrderBy(item => item.Level)
                                .ToList();
                            ttl = BoostCacheTtl;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Miare boost-levels request failed: {Status} {Body}", (int)response.StatusCode, response.Content);
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Miare boost-levels request failed.");
                }
            }

            lock (BoostCacheLock)
            {
                _boostCache = levels;
                _boostCacheExpiresUtc = DateTime.UtcNow + ttl;
            }
            return levels;
        }

        public async Task<ShippingProviderShipmentResult> CreateShipmentAsync(
            ShippingProviderShipmentRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var providerOptions = _options.Miare;
            if (!providerOptions.Enabled)
                return new ShippingProviderShipmentResult
                {
                    IsSuccess = false,
                    ErrorMessage = Resource.Notification.ShippingProviderServiceDisabled
                };

            if (string.IsNullOrWhiteSpace(providerOptions.BaseUrl) || string.IsNullOrWhiteSpace(providerOptions.ApiKey))
                return new ShippingProviderShipmentResult
                {
                    IsSuccess = false,
                    ErrorMessage = string.Format(Resource.Notification.ShippingProviderConnectionSettingsIncompleteFormat, Provider)
                };

            if (request.OriginLatitude is null || request.OriginLongitude is null ||
                request.DestinationLatitude is null || request.DestinationLongitude is null)
                return new ShippingProviderShipmentResult
                {
                    IsSuccess = false,
                    ErrorMessage = string.Format(Resource.Notification.ShippingProviderConnectionSettingsIncompleteFormat, Provider)
                };

            try
            {
                var client = new RestClient(new RestClientOptions(providerOptions.BaseUrl.TrimEnd('/')));

                var body = new MiareCreateTripRequest
                {
                    Pickup = new MiarePickup
                    {
                        // pickup.image برای میاره الزامی است (بدون آن ساخت سفر ۴۰۰ می‌دهد)
                        Image = string.IsNullOrWhiteSpace(request.PickupImageUrl) ? providerOptions.DefaultPickupImageUrl : request.PickupImageUrl,
                        Name = string.IsNullOrWhiteSpace(request.PickupName) ? "فروشگاه" : request.PickupName,
                        PhoneNumber = request.PickupPhone,
                        // آدرس فروشگاه (محل تحویل‌گیری)، نه آدرس گیرنده؛ اگر ثبت نشده بود به آدرس گیرنده برمی‌گردد
                        Address = string.IsNullOrWhiteSpace(request.PickupAddress) ? request.RecipientAddress : request.PickupAddress,
                        Location = new MiareLocation
                        {
                            Latitude = request.OriginLatitude.Value,
                            Longitude = request.OriginLongitude.Value
                        },
                        // ساعت تحویل به پیک را فروشنده تعیین می‌کند؛ بدون آن (مسیر قدیمی) ۳۰ دقیقه بعد
                        // فرمت زمان: میاره آفست با دونقطه («+00:00») را رد می‌کند (parse_error، تست staging) و فقط «Z» یا «+0000» را می‌پذیرد؛ UTC با «Z» می‌فرستیم
                        Deadline = FormatDeadline(request.PickupDeadlineUtc ?? DateTime.UtcNow.AddMinutes(30))
                    },
                    Courses = new[]
                    {
                        new MiareCourse
                        {
                            BillNumber = string.IsNullOrWhiteSpace(request.BillNumber) ? request.OrderId : request.BillNumber,
                            DeliveryCode = string.IsNullOrWhiteSpace(request.DeliveryCode) ? null : request.DeliveryCode,
                            Name = request.RecipientName,
                            PhoneNumber = request.RecipientMobile,
                            Address = request.RecipientAddress,
                            Location = new MiareLocation
                            {
                                Latitude = request.DestinationLatitude.Value,
                                Longitude = request.DestinationLongitude.Value
                            }
                        }
                    }
                };

                // اکسپرس: boost.level (مستند میاره: اگر Boost برای حساب فعال نباشد درخواست boost رد/نادیده گرفته می‌شود)
                var boostRequested = request.BoostLevel.GetValueOrDefault() > 0;
                var boostApplied = boostRequested;
                body.Boost = boostRequested ? new MiareBoost { Level = request.BoostLevel.Value } : null;

                async Task<RestResponse> SendAsync()
                {
                    var restRequest = new RestRequest("/trips/", Method.Post);
                    restRequest.AddHeader("Authorization", $"Token {providerOptions.ApiKey}");
                    restRequest.AddHeader("Content-Type", "application/json");
                    restRequest.AddJsonBody(body);
                    return await client.ExecuteAsync(restRequest, cancellationToken);
                }

                var response = await SendAsync();
                if (boostRequested && !response.IsSuccessful &&
                    (response.Content ?? string.Empty).Contains("boost", StringComparison.OrdinalIgnoreCase))
                {
                    // سفر اکسپرس نباید به‌خاطر boost گیر کند؛ بدون boost دوباره می‌سازیم (پیک با اولویت عادی)
                    _logger.LogWarning("Miare rejected boost level {Level}; creating the express trip without boost. Response: {Body}",
                        request.BoostLevel, response.Content);
                    body.Boost = null;
                    boostApplied = false;
                    response = await SendAsync();
                }

                // مهلت تحویل به پیک نزدیک: «403 {"code":"deadline_too_soon"}»؛ با مهلت دیرتر دوباره می‌سازیم و همان را برمی‌گردانیم
                var usedDeadline = request.PickupDeadlineUtc ?? DateTime.UtcNow.AddMinutes(30);
                foreach (var retryLeadMinutes in new[] { 45, 60 })
                {
                    if (response.IsSuccessful || !IsDeadlineTooSoon(response.Content))
                        break;
                    usedDeadline = DateTime.UtcNow.AddMinutes(retryLeadMinutes);
                    _logger.LogWarning("Miare rejected pickup deadline (deadline_too_soon); retrying with now + {Lead} minutes.", retryLeadMinutes);
                    body.Pickup.Deadline = FormatDeadline(usedDeadline);
                    response = await SendAsync();
                }

                // تصویر فروشگاه (مثلاً webp/آدرس غیرقابل‌دسترس) ممکن است رد شود؛ یک‌بار با تصویر پیش‌فرض دوباره می‌سازیم تا سفر گیر نکند
                if (!response.IsSuccessful &&
                    !string.IsNullOrWhiteSpace(request.PickupImageUrl) &&
                    !string.Equals(body.Pickup.Image, providerOptions.DefaultPickupImageUrl, StringComparison.Ordinal) &&
                    (response.Content ?? string.Empty).Contains("image", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Miare rejected pickup image {Image}; retrying with the default image. Response: {Body}",
                        body.Pickup.Image, response.Content);
                    body.Pickup.Image = providerOptions.DefaultPickupImageUrl;
                    response = await SendAsync();
                }

                if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
                {
                    // دلیل واقعی رد شدن (۴۰۰ اعتبارسنجی فیلدها و...) قبلاً گم می‌شد: فقط detail/code خوانده می‌شد و فروشنده/ادمین
                    // «Miare request failed.» می‌دید. حالا کد HTTP و بدنه‌ی پاسخ (بدون درخواست/کلید) در Shipment.FailureReason و لاگ می‌آید.
                    var reason = DescribeFailure(response);
                    _logger.LogWarning("Miare create trip failed for bill {BillNumber}. {Reason}", body.Courses[0].BillNumber, reason);
                    return new ShippingProviderShipmentResult { IsSuccess = false, ErrorMessage = reason };
                }

                using var doc = JsonDocument.Parse(response.Content);
                var root = doc.RootElement;
                var tripId = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                var trackingUrl = root.TryGetProperty("courses", out var courses) && courses.GetArrayLength() > 0 &&
                                   courses[0].TryGetProperty("tracking_url", out var trackingProp)
                    ? trackingProp.GetString()
                    : null;

                if (string.IsNullOrWhiteSpace(tripId))
                    return new ShippingProviderShipmentResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Miare response did not include a trip id."
                    };

                return new ShippingProviderShipmentResult
                {
                    IsSuccess = true,
                    ExternalShipmentId = tripId,
                    TrackingCode = string.IsNullOrWhiteSpace(trackingUrl) ? tripId : trackingUrl,
                    BoostApplied = boostApplied && BoostWasApplied(root),
                    PickupDeadlineUtc = usedDeadline
                };
            }
            catch (Exception exception)
            {
                // قبلاً بی‌صدا فقط exception.Message («Object reference not set...») ذخیره می‌شد و ردی در لاگ نبود
                _logger.LogError(exception, "Miare create trip threw for bill {BillNumber}.", request.BillNumber);
                return new ShippingProviderShipmentResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"{exception.GetType().Name}: {exception.Message}"
                };
            }
        }

        public async Task<ShippingProviderCancelResult> CancelShipmentAsync(
            ShippingProviderCancelRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var providerOptions = _options.Miare;
            if (!providerOptions.Enabled)
                return ShippingProviderCancelResult.Failed(Resource.Notification.ShippingProviderServiceDisabled);

            if (string.IsNullOrWhiteSpace(providerOptions.BaseUrl) || string.IsNullOrWhiteSpace(providerOptions.ApiKey))
                return ShippingProviderCancelResult.Failed(
                    string.Format(Resource.Notification.ShippingProviderConnectionSettingsIncompleteFormat, Provider));

            if (string.IsNullOrWhiteSpace(request.ExternalShipmentId))
                return ShippingProviderCancelResult.Failed("Missing external shipment id.");

            try
            {
                var client = new RestClient(new RestClientOptions(providerOptions.BaseUrl.TrimEnd('/')));
                var restRequest = new RestRequest($"/trips/{request.ExternalShipmentId}/cancel/", Method.Post);
                restRequest.AddHeader("Authorization", $"Token {providerOptions.ApiKey}");

                var response = await client.ExecuteAsync(restRequest, cancellationToken);
                if (!response.IsSuccessful)
                    return ShippingProviderCancelResult.Failed(
                        ExtractMiareError(response.Content) ?? response.ErrorMessage ?? "Miare cancel request failed.");

                return ShippingProviderCancelResult.Success();
            }
            catch (Exception exception)
            {
                return ShippingProviderCancelResult.Failed(exception.Message);
            }
        }

        // وضعیت فعلی سفر برای جبران وب‌هوک‌های نرسیده (GET Trip طبق مستند میاره).
        public async Task<string> GetTripSnapshotAsync(string tripId, CancellationToken cancellationToken = default)
        {
            var options = _options.Miare;
            if (!options.Enabled || string.IsNullOrWhiteSpace(options.BaseUrl) ||
                string.IsNullOrWhiteSpace(options.ApiKey) || string.IsNullOrWhiteSpace(tripId))
                return null;

            var client = new RestClient(new RestClientOptions(options.BaseUrl.TrimEnd('/')));
            var request = new RestRequest($"/trips/{Uri.EscapeDataString(tripId)}/", Method.Get);
            request.AddHeader("Authorization", $"Token {options.ApiKey}");
            var response = await client.ExecuteAsync(request, cancellationToken);
            if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
            {
                _logger.LogWarning("Miare trip snapshot failed for {TripId}: HTTP {StatusCode}.", tripId, (int)response.StatusCode);
                return null;
            }
            return response.Content;
        }

        // پاسخ میاره: boost: null یعنی نادیده گرفته شد؛ شیء با level>0 یعنی اعمال شد؛ نبودن کلید = نامعلوم (اعمال‌شده فرض می‌شود)
        private static bool BoostWasApplied(JsonElement root)
        {
            if (!root.TryGetProperty("boost", out var boost)) return true;
            if (boost.ValueKind == JsonValueKind.Object)
                return boost.TryGetProperty("level", out var level) && level.ValueKind == JsonValueKind.Number && level.GetInt32() > 0;
            return boost.ValueKind != JsonValueKind.Null;
        }

        private static string ExtractMiareError(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return null;
                if (doc.RootElement.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                    return detail.GetString();
                if (doc.RootElement.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.String)
                    return code.GetString();
            }
            catch (JsonException)
            {
                // پاسخ JSON معتبر نبود، از پیام خام صرف‌نظر می‌کنیم.
            }

            return null;
        }

        private static bool IsDeadlineTooSoon(string content) =>
            !string.IsNullOrWhiteSpace(content) && content.Contains("deadline_too_soon", StringComparison.OrdinalIgnoreCase);

        // «HTTP 400: <پیام/بدنه‌ی خام تا ۶۰۰ نویسه>»؛ خطای ساختاریافته‌ی فیلدها ({"pickup":{"image":["..."]}}) همان بدنه‌ی خام است
        private static string DescribeFailure(RestResponse response)
        {
            var content = response.Content?.Trim();
            var message = ExtractMiareError(content);
            if (string.IsNullOrWhiteSpace(message))
                message = string.IsNullOrWhiteSpace(content)
                    ? response.ErrorMessage ?? "Miare request failed."
                    : content;
            if (message.Length > 600)
                message = message[..600];
            return $"HTTP {(int)response.StatusCode}: {message}";
        }

        private class MiareCreateTripRequest
        {
            [JsonPropertyName("pickup")]
            public MiarePickup Pickup { get; set; }

            [JsonPropertyName("courses")]
            public MiareCourse[] Courses { get; set; }

            [JsonPropertyName("boost")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public MiareBoost Boost { get; set; }
        }

        private class MiareBoost
        {
            [JsonPropertyName("level")]
            public int Level { get; set; }
        }

        private class MiarePickup
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("phone_number")]
            public string PhoneNumber { get; set; }

            [JsonPropertyName("address")]
            public string Address { get; set; }

            [JsonPropertyName("location")]
            public MiareLocation Location { get; set; }

            [JsonPropertyName("deadline")]
            public string Deadline { get; set; }

            [JsonPropertyName("image")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string Image { get; set; }
        }

        private class MiareCourse
        {
            [JsonPropertyName("bill_number")]
            public string BillNumber { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("phone_number")]
            public string PhoneNumber { get; set; }

            [JsonPropertyName("address")]
            public string Address { get; set; }

            [JsonPropertyName("delivery_code")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string DeliveryCode { get; set; }

            [JsonPropertyName("location")]
            public MiareLocation Location { get; set; }
        }

        private class MiareLocation
        {
            [JsonPropertyName("latitude")]
            public double Latitude { get; set; }

            [JsonPropertyName("longitude")]
            public double Longitude { get; set; }
        }
    }
}
