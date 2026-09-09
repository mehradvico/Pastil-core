using Application.Common.Dto.LocationPoint;
using Application.Common.Dto.Result;
using Application.Common.Geography.Dto;
using Application.Common.Geography.Iface;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
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
        private readonly string _osrmBaseUrl;
        private readonly string _photonBaseUrl;

        public OsrmPhotonGeographyService(IConfiguration configuration)
        {
            _osrmBaseUrl = configuration["Osrm:BaseUrl"] ?? "http://osrm:5000";
            _photonBaseUrl = configuration["Photon:BaseUrl"] ?? "http://photon:2322";
        }

        private static string FormatCoord(double value) =>
            value.ToString(CultureInfo.InvariantCulture);

        public async Task<double> GetDrivingDistanceAsync(PointDto start, PointDto end, bool kmResult = true, bool roundResult = true)
        {
            var options = new RestClientOptions(_osrmBaseUrl)
            {
                Timeout = System.Threading.Timeout.InfiniteTimeSpan,
            };
            var client = new RestClient(options);
            var path = $"/route/v1/driving/{FormatCoord(start.x)},{FormatCoord(start.y)};{FormatCoord(end.x)},{FormatCoord(end.y)}?alternatives=false&steps=false";
            var request = new RestRequest(path, Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            using JsonDocument doc = JsonDocument.Parse(response.Content);

            if (!doc.RootElement.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
                throw new InvalidOperationException($"OSRM driving route request failed: {response.Content}");

            double distance = routes[0].GetProperty("distance").GetDouble();
            if (kmResult)
                distance /= 1000;
            if (roundResult)
                distance = Math.Ceiling(distance);
            return distance;
        }

        public async Task<List<PointDto>> GetDrivingRouteAsync(PointDto start, PointDto end)
        {
            var options = new RestClientOptions(_osrmBaseUrl)
            {
                Timeout = System.Threading.Timeout.InfiniteTimeSpan,
            };
            var client = new RestClient(options);
            var path = $"/route/v1/driving/{FormatCoord(start.x)},{FormatCoord(start.y)};{FormatCoord(end.x)},{FormatCoord(end.y)}?alternatives=false&steps=false&geometries=geojson&overview=full";
            var request = new RestRequest(path, Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            using JsonDocument doc = JsonDocument.Parse(response.Content);

            if (!doc.RootElement.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
                throw new InvalidOperationException($"OSRM driving route request failed: {response.Content}");

            var coordinates = routes[0].GetProperty("geometry").GetProperty("coordinates");
            var result = new List<PointDto>();
            foreach (var coordinate in coordinates.EnumerateArray())
            {
                result.Add(new PointDto(coordinate[0].GetDouble(), coordinate[1].GetDouble()));
            }
            return result;
        }

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
