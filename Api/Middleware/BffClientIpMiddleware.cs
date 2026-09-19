using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Api.Middleware
{
    /// <summary>
    /// Lets the Nuxt BFF (webapp server) vouch for the real end-user IP.
    /// The webapp calls this API from its own server, so without this every visitor
    /// shares one IP and per-IP rate limits (OTP, sign-in, ...) would throttle the
    /// whole site at once. The IP is only honoured when the request also carries the
    /// shared secret, so an anonymous caller cannot pick its own rate-limit bucket.
    /// The secret is read from Security:ClientIpAttestationKey (env PASTIL_CLIENT_IP_ATTESTATION_KEY);
    /// when it is missing or shorter than 32 characters the middleware does nothing.
    /// </summary>
    public sealed class BffClientIpMiddleware
    {
        public const string ClientIpHeader = "X-Pastil-Client-Ip";
        public const string KeyHeader = "X-Pastil-Client-Ip-Key";
        private const int MinimumKeyLength = 32;

        private readonly RequestDelegate _next;
        private readonly byte[] _keyBytes;

        public BffClientIpMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<BffClientIpMiddleware> logger)
        {
            _next = next;

            var key = configuration["Security:ClientIpAttestationKey"]?.Trim();
            if (string.IsNullOrEmpty(key))
            {
                _keyBytes = Array.Empty<byte>();
            }
            else if (key.Length < MinimumKeyLength)
            {
                _keyBytes = Array.Empty<byte>();
                logger.LogWarning("Security:ClientIpAttestationKey is shorter than {Minimum} characters and was ignored.", MinimumKeyLength);
            }
            else
            {
                _keyBytes = Encoding.UTF8.GetBytes(key);
            }
        }

        public Task InvokeAsync(HttpContext context)
        {
            if (_keyBytes.Length > 0 && TryGetAttestedAddress(context, out var address))
            {
                context.Connection.RemoteIpAddress = address;
            }

            return _next(context);
        }

        private bool TryGetAttestedAddress(HttpContext context, out IPAddress address)
        {
            address = IPAddress.None;

            var headers = context.Request.Headers;
            var providedKey = headers[KeyHeader].ToString();
            var providedIp = headers[ClientIpHeader].ToString();
            if (string.IsNullOrWhiteSpace(providedKey) || string.IsNullOrWhiteSpace(providedIp))
            {
                return false;
            }

            var providedKeyBytes = Encoding.UTF8.GetBytes(providedKey.Trim());
            if (!CryptographicOperations.FixedTimeEquals(providedKeyBytes, _keyBytes))
            {
                return false;
            }

            if (!IPAddress.TryParse(providedIp.Trim(), out var parsed))
            {
                return false;
            }

            address = parsed.IsIPv4MappedToIPv6 ? parsed.MapToIPv4() : parsed;
            return true;
        }
    }
}
