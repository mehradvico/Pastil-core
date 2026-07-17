using Application.Common.Dto.LocationPoint;
using Application.Common.Dto.Result;
using Application.Common.Geography.Dto;
using Application.Common.Geography.Iface;
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
        public async Task<double> GetDrivingDistanceAsync(PointDto start, PointDto end, bool kmResult = true, bool roundResult = true)
        {
            var options = new RestClientOptions("https://map.ir")
            {
                Timeout = System.Threading.Timeout.InfiniteTimeSpan,
            };
            var client = new RestClient(options);
            var request1 = $"/routes/route/v1/driving/{(start.x).ToString().Replace("٫", ".")},{start.y.ToString().Replace("٫", ".")};{end.x.ToString().Replace("٫", ".")},{end.y.ToString().Replace("٫", ".")}?alternatives=false&steps=false";
            var request = new RestRequest(request1, Method.Get);
            request.AddHeader("x-api-key", "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImp0aSI6IjY2MDQ1MWM2ZGZhYjJjZWY1ZWY3NmQ3NmZiMTI2OTRlYjA5ZjAwYTEwMDg5MjgxMGUyMjlhMTM4OWEzYmE3MzVhNWQ1MDJkZmIxZDg4ZTViIn0.eyJhdWQiOiIyNDkxNCIsImp0aSI6IjY2MDQ1MWM2ZGZhYjJjZWY1ZWY3NmQ3NmZiMTI2OTRlYjA5ZjAwYTEwMDg5MjgxMGUyMjlhMTM4OWEzYmE3MzVhNWQ1MDJkZmIxZDg4ZTViIiwiaWF0IjoxNjk5OTQ0MDkwLCJuYmYiOjE2OTk5NDQwOTAsImV4cCI6MTcwMjQ0OTY5MCwic3ViIjoiIiwic2NvcGVzIjpbImJhc2ljIl19.VU7ow2n6-MQ0yFh7zyjrYx0YwmYK7obdlSvVxA180agkhZFuZY_IVvrOmw5sy7peft1QS75sOQ1Ptu-w46nS-B_iFn1G-5SEbpeU-S_Eh5EUE0p5M89bsDz9nEYjwtpqRizYABaJBByvrpXlXiOZMKS7FrUQO3Ky0zGGnT6AMpL39F8fynxcp7xpKrsY-GsV3haNNt1axu9JFBE739hEmu451siTdg5C_hHI1SFB5smn4Dx4kZAsmuJFMa_Kywq9rSeDXI033RCXrkZzpwGjJaJ2u4UkOqB4MPfrbLQkmSlMZLY1DsKZF3L210_uYHhCQ2EggKfYwYMyw8S2mWC80A");
            RestResponse response = await client.ExecuteAsync(request);
            using JsonDocument doc = JsonDocument.Parse(response.Content);

            double distance = doc.RootElement.GetProperty("routes")[0].GetProperty("distance").GetDouble();
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
            var options = new RestClientOptions("https://map.ir")
            {
                Timeout = System.Threading.Timeout.InfiniteTimeSpan,
            };
            var client = new RestClient(options);
            var request1 = $"/search/v2/?text={Uri.EscapeDataString(q)}";
            var request = new RestRequest(request1, Method.Get);
            request.AddHeader("x-api-key", "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImp0aSI6IjY2MDQ1MWM2ZGZhYjJjZWY1ZWY3NmQ3NmZiMTI2OTRlYjA5ZjAwYTEwMDg5MjgxMGUyMjlhMTM4OWEzYmE3MzVhNWQ1MDJkZmIxZDg4ZTViIn0.eyJhdWQiOiIyNDkxNCIsImp0aSI6IjY2MDQ1MWM2ZGZhYjJjZWY1ZWY3NmQ3NmZiMTI2OTRlYjA5ZjAwYTEwMDg5MjgxMGUyMjlhMTM4OWEzYmE3MzVhNWQ1MDJkZmIxZDg4ZTViIiwiaWF0IjoxNjk5OTQ0MDkwLCJuYmYiOjE2OTk5NDQwOTAsImV4cCI6MTcwMjQ0OTY5MCwic3ViIjoiIiwic2NvcGVzIjpbImJhc2ljIl19.VU7ow2n6-MQ0yFh7zyjrYx0YwmYK7obdlSvVxA180agkhZFuZY_IVvrOmw5sy7peft1QS75sOQ1Ptu-w46nS-B_iFn1G-5SEbpeU-S_Eh5EUE0p5M89bsDz9nEYjwtpqRizYABaJBByvrpXlXiOZMKS7FrUQO3Ky0zGGnT6AMpL39F8fynxcp7xpKrsY-GsV3haNNt1axu9JFBE739hEmu451siTdg5C_hHI1SFB5smn4Dx4kZAsmuJFMa_Kywq9rSeDXI033RCXrkZzpwGjJaJ2u4UkOqB4MPfrbLQkmSlMZLY1DsKZF3L210_uYHhCQ2EggKfYwYMyw8S2mWC80A");
            RestResponse response = await client.ExecuteAsync(request);
            var json = JsonSerializer.Deserialize<MapIrResponseDto>(response.Content);
            if (json?.value == null || json.value.Length == 0)
            {
                return new BaseResultDto<List<MapIrResultDto>>(false, data: null, val: Resource.Notification.NothingFound);
            }
            var results = json.value.Select(item => new MapIrResultDto
            {
                Address = item.address,
                Location = new PointDto(item.geom.coordinates[1], item.geom.coordinates[0])

            }).ToList();

            return new BaseResultDto<List<MapIrResultDto>>(true, results);


        }
    }
}
