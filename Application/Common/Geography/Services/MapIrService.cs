using Application.Common.Dto.LocationPoint;
using Application.Common.Dto.Result;
using Application.Common.Geography.Dto;
using Application.Common.Geography.Iface;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Common.Geography.Services
{
    public class MapIrService : IGeographyService
    {
        private readonly string _apiKey;

        public MapIrService(IConfiguration configuration)
        {
            _apiKey = configuration["MapIr:ApiKey"];
        }

        public async Task<double> GetDrivingDistanceAsync(PointDto start, PointDto end, bool kmResult = true, bool roundResult = true)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("MapIr:ApiKey is not configured.");

            var options = new RestClientOptions("https://map.ir")
            {
                Timeout = System.Threading.Timeout.InfiniteTimeSpan,
            };
            var client = new RestClient(options);
            var request1 = $"/routes/route/v1/driving/{(start.x).ToString().Replace("٫", ".")},{start.y.ToString().Replace("٫", ".")};{end.x.ToString().Replace("٫", ".")},{end.y.ToString().Replace("٫", ".")}?alternatives=false&steps=false";
            var request = new RestRequest(request1, Method.Get);
            request.AddHeader("x-api-key", _apiKey);
            RestResponse response = await client.ExecuteAsync(request);
            using JsonDocument doc = JsonDocument.Parse(response.Content);

            if (!doc.RootElement.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
                throw new InvalidOperationException($"map.ir driving route request failed: {response.Content}");

            double distance = routes[0].GetProperty("distance").GetDouble();
            if (kmResult)
                distance /= 1000;
            if (roundResult)
                distance = Math.Ceiling(distance);
            return distance;

        }

        public async Task<BaseResultDto<List<MapIrResultDto>>> SearchAsync(string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                return new BaseResultDto<List<MapIrResultDto>>(false, data: null, val: Resource.Notification.NothingFound);
            }
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return new BaseResultDto<List<MapIrResultDto>>(false, data: null, val: Resource.Notification.NothingFound);
            }
            var options = new RestClientOptions("https://map.ir")
            {
                Timeout = System.Threading.Timeout.InfiniteTimeSpan,
            };
            var client = new RestClient(options);
            var request1 = $"/search/v2/?text={Uri.EscapeDataString(q)}";
            var request = new RestRequest(request1, Method.Get);
            request.AddHeader("x-api-key", _apiKey);
            RestResponse response = await client.ExecuteAsync(request);
            var json = JsonSerializer.Deserialize<MapIrResponseDto>(response.Content);
            if (json?.value == null || json.value.Length == 0)
            {
                return new BaseResultDto<List<MapIrResultDto>>(false, data: null, val: Resource.Notification.NothingFound);
            }
            var results = json.value.Select(item => new MapIrResultDto
            {
                Address = item.address,
                Location = new PointDto(item.geom.coordinates[0], item.geom.coordinates[1])

            }).ToList();

            return new BaseResultDto<List<MapIrResultDto>>(true, results);


        }
    }
}
