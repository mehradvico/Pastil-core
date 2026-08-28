using Entities.Entities.ShippingField;
using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv.Provider
{
    // میاره (Miare) سرویس واقعی پیک/ارسال است — برخلاف سایر ارائه‌دهنده‌ها (AloPeyk/Tipax/SnappBox)
    // که هنوز فقط حالت تست دارند، این کلاس مستقیماً با API واقعی میاره صحبت می‌کند.
    // میاره برخلاف انتزاع فعلی، endpoint استعلام قیمت جداگانه ندارد؛ قیمت واقعی («delivery_cost»)
    // فقط بعد از تحویل نهایی مشخص می‌شود، پس GetQuoteAsync یک برآورد محلی برمی‌گرداند، نه استعلام واقعی.
    public class MiareShippingProvider : IShippingProvider
    {
        private readonly ShippingOptions _options;

        public MiareShippingProvider(IOptions<ShippingOptions> options)
        {
            _options = options.Value;
        }

        public ShippingProviderEnum Provider => ShippingProviderEnum.Miare;

        public Task<ShippingProviderQuoteResult> GetQuoteAsync(
            ShippingProviderQuoteRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_options.Miare.Enabled)
                return Task.FromResult(ShippingProviderQuoteResult.Failed(Resource.Notification.ShippingProviderServiceDisabled));

            // میاره قیمت واقعی سفر را فقط بعد از تحویل اعلام می‌کند، نه در زمان استعلام.
            // اینجا فقط یک شناسه‌ی موقت برمی‌گردانیم تا جریان استعلام/انتخاب ارائه‌دهنده مثل بقیه کار کند؛
            // مبلغ نهایی واقعی را باید بعداً (مثلاً از طریق وب‌هوک یا استعلام وضعیت سفر) به‌روزرسانی کرد.
            return Task.FromResult(new ShippingProviderQuoteResult
            {
                IsSuccess = true,
                Price = 0,
                Currency = "IRR",
                ExternalQuoteId = $"MIARE-PENDING-{Guid.NewGuid():N}"
            });
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
                var restRequest = new RestRequest("/trips/", Method.Post);
                restRequest.AddHeader("Authorization", $"Token {providerOptions.ApiKey}");
                restRequest.AddHeader("Content-Type", "application/json");

                var body = new MiareCreateTripRequest
                {
                    Pickup = new MiarePickup
                    {
                        Name = string.IsNullOrWhiteSpace(request.PickupName) ? "فروشگاه" : request.PickupName,
                        PhoneNumber = request.PickupPhone,
                        Address = request.RecipientAddress,
                        Location = new MiareLocation
                        {
                            Latitude = request.OriginLatitude.Value,
                            Longitude = request.OriginLongitude.Value
                        },
                        Deadline = DateTimeOffset.Now.AddMinutes(30).ToString("yyyy-MM-ddTHH:mm:sszzz")
                    },
                    Courses = new[]
                    {
                        new MiareCourse
                        {
                            BillNumber = request.OrderId,
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

                restRequest.AddJsonBody(body);
                var response = await client.ExecuteAsync(restRequest, cancellationToken);

                if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
                    return new ShippingProviderShipmentResult
                    {
                        IsSuccess = false,
                        ErrorMessage = ExtractMiareError(response.Content) ?? response.ErrorMessage ?? "Miare request failed."
                    };

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
                    TrackingCode = string.IsNullOrWhiteSpace(trackingUrl) ? tripId : trackingUrl
                };
            }
            catch (Exception exception)
            {
                return new ShippingProviderShipmentResult
                {
                    IsSuccess = false,
                    ErrorMessage = exception.Message
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

        private static string ExtractMiareError(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("detail", out var detail))
                    return detail.GetString();
                if (doc.RootElement.TryGetProperty("code", out var code))
                    return code.GetString();
            }
            catch (JsonException)
            {
                // پاسخ JSON معتبر نبود، از پیام خام صرف‌نظر می‌کنیم.
            }

            return null;
        }

        private class MiareCreateTripRequest
        {
            [JsonPropertyName("pickup")]
            public MiarePickup Pickup { get; set; }

            [JsonPropertyName("courses")]
            public MiareCourse[] Courses { get; set; }
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
