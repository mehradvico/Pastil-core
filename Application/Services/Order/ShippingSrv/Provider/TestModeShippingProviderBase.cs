using Entities.Entities.ShippingField;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv.Provider
{
    public abstract class TestModeShippingProviderBase : IShippingProvider
    {
        private readonly ShippingOptions _options;

        protected TestModeShippingProviderBase(IOptions<ShippingOptions> options)
        {
            _options = options.Value;
        }

        public abstract ShippingProviderEnum Provider { get; }
        protected abstract double BasePrice { get; }
        protected abstract double PricePerKilometer { get; }
        protected virtual double PricePerKilogram => 0;

        public Task<ShippingProviderQuoteResult> GetQuoteAsync(
            ShippingProviderQuoteRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var providerOptions = GetProviderOptions();
            if (!providerOptions.Enabled)
                return Task.FromResult(ShippingProviderQuoteResult.Failed("این سرویس ارسال غیرفعال است."));

            if (!_options.TestMode)
            {
                if (string.IsNullOrWhiteSpace(providerOptions.BaseUrl) ||
                    string.IsNullOrWhiteSpace(providerOptions.ApiKey))
                    return Task.FromResult(ShippingProviderQuoteResult.Failed(
                        $"تنظیمات اتصال {Provider} تکمیل نشده است."));

                return Task.FromResult(ShippingProviderQuoteResult.Failed(
                    $"قرارداد API عملیاتی {Provider} باید پس از دریافت مستند رسمی Provider پیاده‌سازی شود."));
            }

            var distance = CalculateDistanceKilometers(request);
            var weight = Math.Max(1, Math.Ceiling(request.WeightGrams / 1000d));
            var price = BasePrice + (distance * PricePerKilometer) + (weight * PricePerKilogram);
            price = Math.Ceiling(price / 1000d) * 1000d;

            return Task.FromResult(new ShippingProviderQuoteResult
            {
                IsSuccess = true,
                Price = price,
                Currency = "IRR",
                ExternalQuoteId = $"TEST-{Provider}-{Guid.NewGuid():N}"
            });
        }

        public Task<ShippingProviderShipmentResult> CreateShipmentAsync(
            ShippingProviderShipmentRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_options.TestMode)
                return Task.FromResult(new ShippingProviderShipmentResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"قرارداد API عملیاتی {Provider} هنوز تنظیم نشده است."
                });

            var id = $"TEST-{Provider}-{Guid.NewGuid():N}";
            return Task.FromResult(new ShippingProviderShipmentResult
            {
                IsSuccess = true,
                ExternalShipmentId = id,
                TrackingCode = id
            });
        }

        private ShippingProviderOptions GetProviderOptions() => Provider switch
        {
            ShippingProviderEnum.AloPeyk => _options.AloPeyk,
            ShippingProviderEnum.Tipax => _options.Tipax,
            ShippingProviderEnum.SnappBox => _options.SnappBox,
            _ => new ShippingProviderOptions { Enabled = false }
        };

        private static double CalculateDistanceKilometers(ShippingProviderQuoteRequest request)
        {
            const double earthRadius = 6371;
            var latitudeDistance = ToRadians(request.DestinationLatitude - request.OriginLatitude);
            var longitudeDistance = ToRadians(request.DestinationLongitude - request.OriginLongitude);
            var originLatitude = ToRadians(request.OriginLatitude);
            var destinationLatitude = ToRadians(request.DestinationLatitude);
            var value = Math.Sin(latitudeDistance / 2) * Math.Sin(latitudeDistance / 2) +
                Math.Cos(originLatitude) * Math.Cos(destinationLatitude) *
                Math.Sin(longitudeDistance / 2) * Math.Sin(longitudeDistance / 2);

            return earthRadius * 2 * Math.Atan2(Math.Sqrt(value), Math.Sqrt(1 - value));
        }

        private static double ToRadians(double value) => value * Math.PI / 180;
    }
}
