using Application.Common.Dto.LocationPoint;
using Application.Common.Dto.Result;
using Application.Common.Geography.Dto;
using Application.Common.Geography.Iface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Common.Geography.Services
{
    // جایگزین MapIrService: مسیریابی از OSRM خودمیزبان (بدون سهمیه) و جست‌وجوی آدرس از Photon
    // خودمیزبان می‌خواند - هر دو روی iran.osm.pbf ساخته شده‌اند (به جای map.ir که هر ماه توکنش تمام می‌شد).
    public class OsrmPhotonGeographyService : IGeographyService
    {
        private const string DistanceCachePrefix = "osrm-distance:";
        private static readonly ConcurrentDictionary<string, Task<double>> InFlightDistances = new();
        private readonly string _photonBaseUrl;
        private readonly IMemoryCache _cache;
        private readonly OsrmRequestCoordinator _requestCoordinator;
        private readonly TimeSpan _distanceCacheDuration;

        public OsrmPhotonGeographyService(
            IConfiguration configuration,
            IMemoryCache cache,
            OsrmRequestCoordinator requestCoordinator)
        {
            _photonBaseUrl = configuration["Photon:BaseUrl"] ?? "http://photon:2322";
            _cache = cache;
            _requestCoordinator = requestCoordinator;
            _distanceCacheDuration = TimeSpan.FromSeconds(Math.Clamp(configuration.GetValue<int?>("Osrm:DistanceCacheSeconds") ?? 600, 30, 3600));
        }

        private static string FormatCoord(double value) =>
            value.ToString(CultureInfo.InvariantCulture);

        public async Task<double> GetDrivingDistanceAsync(PointDto start, PointDto end, bool kmResult = true, bool roundResult = true)
        {
            ValidatePoint(start);
            ValidatePoint(end);

            var cacheKey = BuildDistanceCacheKey(start, end);
            if (!_cache.TryGetValue(cacheKey, out double distance))
            {
                var inFlight = InFlightDistances.GetOrAdd(cacheKey, _ => GetDrivingDistanceInMetersAsync(start, end));
                try
                {
                    distance = await inFlight;
                    _cache.Set(cacheKey, distance, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _distanceCacheDuration,
                        Size = 1
                    });
                }
                finally
                {
                    if (inFlight.IsCompleted)
                    {
                        ((ICollection<KeyValuePair<string, Task<double>>>)InFlightDistances)
                            .Remove(new KeyValuePair<string, Task<double>>(cacheKey, inFlight));
                    }
                }
            }

            if (kmResult)
                distance /= 1000;
            if (roundResult)
                distance = Math.Ceiling(distance);
            return distance;
        }

        public async Task<List<PointDto>> GetDrivingRouteAsync(PointDto start, PointDto end)
        {
            ValidatePoint(start);
            ValidatePoint(end);
            var path = $"/route/v1/driving/{FormatCoord(start.x)},{FormatCoord(start.y)};{FormatCoord(end.x)},{FormatCoord(end.y)}?alternatives=false&steps=false&geometries=geojson&overview=full";
            using JsonDocument doc = await ExecuteOsrmAsync(path);

            if (!doc.RootElement.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
                throw new GeographyDependencyUnavailableException("OSRM did not return a driving route.");

            var coordinates = routes[0].GetProperty("geometry").GetProperty("coordinates");
            var result = new List<PointDto>();
            foreach (var coordinate in coordinates.EnumerateArray())
            {
                result.Add(new PointDto(coordinate[0].GetDouble(), coordinate[1].GetDouble()));
            }
            return result;
        }

        private async Task<double> GetDrivingDistanceInMetersAsync(PointDto start, PointDto end)
        {
            var path = $"/route/v1/driving/{FormatCoord(start.x)},{FormatCoord(start.y)};{FormatCoord(end.x)},{FormatCoord(end.y)}?alternatives=false&steps=false";
            using JsonDocument doc = await ExecuteOsrmAsync(path);
            if (!doc.RootElement.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
                throw new GeographyDependencyUnavailableException("OSRM did not return a driving route.");

            return routes[0].GetProperty("distance").GetDouble();
        }

        private async Task<JsonDocument> ExecuteOsrmAsync(string path)
        {
            var response = await _requestCoordinator.ExecuteOsrmAsync(new RestRequest(path, Method.Get));
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
            {
                throw new GeographyDependencyUnavailableException("OSRM returned an unsuccessful response.");
            }

            try
            {
                return JsonDocument.Parse(response.Content);
            }
            catch (JsonException exception)
            {
                throw new GeographyDependencyUnavailableException("OSRM returned an invalid response.", exception);
            }
        }

        private static void ValidatePoint(PointDto point)
        {
            if (point == null || !double.IsFinite(point.x) || !double.IsFinite(point.y) || point.x is < -180 or > 180 || point.y is < -90 or > 90)
            {
                throw new ArgumentException("A valid longitude and latitude are required for routing.");
            }
        }

        private static string BuildDistanceCacheKey(PointDto start, PointDto end) =>
            $"{DistanceCachePrefix}{start.x:F6},{start.y:F6}:{end.x:F6},{end.y:F6}";

        public async Task<BaseResultDto<List<MapIrResultDto>>> SearchAsync(string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                return new BaseResultDto<List<MapIrResultDto>>(false, data: null, val: Resource.Notification.NothingFound);
            }

            var options = new RestClientOptions(_photonBaseUrl)
            {
                Timeout = System.Threading.Timeout.InfiniteTimeSpan,
            };
            var client = new RestClient(options);
            var path = $"/api?q={Uri.EscapeDataString(q)}&lang=fa&limit=10";
            var request = new RestRequest(path, Method.Get);
            RestResponse response = await client.ExecuteAsync(request);

            using JsonDocument doc = JsonDocument.Parse(response.Content);
            if (!doc.RootElement.TryGetProperty("features", out var features) || features.GetArrayLength() == 0)
            {
                return new BaseResultDto<List<MapIrResultDto>>(false, data: null, val: Resource.Notification.NothingFound);
            }

            var results = features.EnumerateArray().Select(feature =>
            {
                var props = feature.GetProperty("properties");
                var coords = feature.GetProperty("geometry").GetProperty("coordinates");

                string Get(string key) => props.TryGetProperty(key, out var v) ? v.GetString() : null;
                var addressParts = new[] { Get("name"), Get("street"), Get("district"), Get("city"), Get("state") }
                    .Where(part => !string.IsNullOrWhiteSpace(part))
                    .Distinct();

                return new MapIrResultDto
                {
                    Address = string.Join("، ", addressParts),
                    Location = new PointDto(coords[0].GetDouble(), coords[1].GetDouble())
                };
            }).ToList();

            return new BaseResultDto<List<MapIrResultDto>>(true, results);
        }
    }
}
