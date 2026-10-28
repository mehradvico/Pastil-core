using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services.CommonSrv.CallIceSrv
{
    public record CallIceServerVDto(string[] Urls, string Username = null, string Credential = null);

    // ساخت iceServers یکسان برای همه‌ی کلاینت‌های تماس (وب‌اپ و اپ). بدون TURN، دو طرف پشت NAT/CGNAT موبایل
    // سیگنالینگ را رد و بدل می‌کنند ولی مسیر رسانه ساخته نمی‌شود و هر کس فقط دوربین خودش را می‌بیند.
    public static class CallIceRules
    {
        public static List<CallIceServerVDto> Build(CallIceOptions options, long userId, DateTime utcNow)
        {
            var list = new List<CallIceServerVDto>();
            var stun = Clean(options?.StunUrls);
            if (stun.Length > 0)
                list.Add(new CallIceServerVDto(stun));

            var turn = Clean(options?.TurnUrls);
            if (turn.Length == 0)
                return list;

            if (!string.IsNullOrWhiteSpace(options.TurnSharedSecret))
            {
                // قرارداد REST کوترن: username = «زمان انقضا (unix):شناسه»، credential = base64(HMAC-SHA1(secret, username))
                var expiry = new DateTimeOffset(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc))
                    .AddMinutes(Math.Max(1, options.CredentialTtlMinutes)).ToUnixTimeSeconds();
                var username = $"{expiry}:{userId}";
                using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(options.TurnSharedSecret));
                var credential = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(username)));
                list.Add(new CallIceServerVDto(turn, username, credential));
            }
            else if (!string.IsNullOrWhiteSpace(options.TurnUsername) && !string.IsNullOrWhiteSpace(options.TurnCredential))
            {
                list.Add(new CallIceServerVDto(turn, options.TurnUsername, options.TurnCredential));
            }

            return list;
        }

        private static string[] Clean(string[] urls) =>
            (urls ?? Array.Empty<string>()).Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => u.Trim()).Distinct().ToArray();
    }
}
