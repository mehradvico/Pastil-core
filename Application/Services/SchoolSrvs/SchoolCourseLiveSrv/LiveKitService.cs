using Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Iface;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolCourseLiveSrv
{
    // پوشش نازک روی LiveKit Cloud: تولید توکن JWT ورود (استاندارد LiveKit access token) + فراخوانی
    // Twirp RPCهای RoomService/Egress برای ساخت/حذف اتاق و شروع/توقف ضبط. مقادیر واقعیِ کلید/سکرت
    // در appsettings/Environment تنظیم می‌شوند (LiveKitOptions) - اینجا هیچ‌کدام هاردکد نیست.
    //
    // نکته برای تیم عملیات: نام‌گذاری دقیق فیلدهای JSON این RPCها مطابق مستندات رسمی LiveKit در
    // زمان نوشتن این کد است؛ قبل از اتصال کلیدهای واقعی، حتماً با یک تماس آزمایشی (مثلاً CreateRoom
    // روی یک روم تستی) تأیید شود که نسخه‌ی فعلی LiveKit Cloud همین قرارداد را می‌پذیرد.
    public class LiveKitService : ILiveKitService
    {
        private readonly LiveKitOptions _options;
        private readonly HttpClient _httpClient;

        public LiveKitService(IOptions<LiveKitOptions> options, IHttpClientFactory httpClientFactory)
        {
            _options = options.Value;
            _httpClient = httpClientFactory.CreateClient("LiveKit");
        }

        public string GenerateAccessToken(string roomName, string identity, string displayName, bool canPublish, bool canSubscribe, int ttlMinutes = 180)
        {
            var videoGrant = new Dictionary<string, object>
            {
                ["room"] = roomName,
                ["roomJoin"] = true,
                ["canPublish"] = canPublish,
                ["canSubscribe"] = canSubscribe,
                ["canPublishData"] = true,
                ["canUpdateOwnMetadata"] = true
            };

            var now = DateTimeOffset.UtcNow;
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.ApiSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("video", JsonSerializer.Serialize(videoGrant), JsonClaimValueTypes.Json),
                new Claim(JwtRegisteredClaimNames.Sub, identity),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };
            if (!string.IsNullOrWhiteSpace(displayName))
                claims.Add(new Claim("name", displayName));

            var token = new JwtSecurityToken(
                issuer: _options.ApiKey,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: now.AddMinutes(ttlMinutes).UtcDateTime,
                signingCredentials: credentials);

            return handler.WriteToken(token);
        }

        // Twirp APIهای LiveKit با یک توکن سرور-به-سرور (grant: roomCreate/roomAdmin/roomRecord) احراز می‌شوند
        private string GenerateServerToken(params string[] grantNames)
        {
            var videoGrant = new Dictionary<string, object>();
            foreach (var grant in grantNames)
                videoGrant[grant] = true;

            var now = DateTimeOffset.UtcNow;
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.ApiSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("video", JsonSerializer.Serialize(videoGrant), JsonClaimValueTypes.Json),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            var token = new JwtSecurityToken(
                issuer: _options.ApiKey,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: now.AddMinutes(10).UtcDateTime,
                signingCredentials: credentials);

            return handler.WriteToken(token);
        }

        private async Task<JsonDocument> CallTwirpAsync(string service, string method, object body, string serverToken)
        {
            var url = $"{_options.Host.TrimEnd('/')}/twirp/{service}/{method}";
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serverToken);

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"LiveKit {service}/{method} failed ({(int)response.StatusCode}): {responseBody}");

            return string.IsNullOrWhiteSpace(responseBody) ? null : JsonDocument.Parse(responseBody);
        }

        public async Task CreateRoomAsync(string roomName)
        {
            var token = GenerateServerToken("roomCreate");
            await CallTwirpAsync("livekit.RoomService", "CreateRoom", new { name = roomName, empty_timeout = 600 }, token);
        }

        public async Task DeleteRoomAsync(string roomName)
        {
            var token = GenerateServerToken("roomCreate");
            try
            {
                await CallTwirpAsync("livekit.RoomService", "DeleteRoom", new { room = roomName }, token);
            }
            catch (InvalidOperationException)
            {
                // اتاقی که از قبل بسته/پاک شده - نادیده گرفته می‌شود، پایان لایو نباید به همین خاطر شکست بخورد
            }
        }

        public async Task<string> StartRecordingAsync(string roomName)
        {
            var token = GenerateServerToken("roomRecord");
            var body = new
            {
                room_name = roomName,
                file_outputs = new[]
                {
                    new
                    {
                        file_type = "MP4",
                        filepath = $"school-live/{roomName}/{DateTime.UtcNow:yyyyMMddHHmmss}.mp4",
                        s3 = new
                        {
                            access_key = _options.RecordingAccessKey,
                            secret = _options.RecordingSecret,
                            region = _options.RecordingRegion,
                            bucket = _options.RecordingBucket
                        }
                    }
                }
            };
            var result = await CallTwirpAsync("livekit.Egress", "StartRoomCompositeEgress", body, token);
            return result?.RootElement.GetProperty("egress_id").GetString();
        }

        public async Task StopRecordingAsync(string egressId)
        {
            if (string.IsNullOrWhiteSpace(egressId))
                return;
            var token = GenerateServerToken("roomRecord");
            await CallTwirpAsync("livekit.Egress", "StopEgress", new { egress_id = egressId }, token);
        }
    }
}
