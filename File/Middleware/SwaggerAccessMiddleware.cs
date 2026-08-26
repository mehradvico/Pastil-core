using System.Net;
using System.Text;

namespace File.Middleware
{
    public class SwaggerCredential
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Gates /swagger (UI + swagger.json) behind HTTP Basic Auth, with an optional
    /// IP allowlist layered on top. Fails closed: if no credentials are configured
    /// at all, /swagger returns 404 regardless of anything else. Real credential
    /// values must come from environment variables on the server, never from a
    /// committed appsettings file — see SWAGGER_ACCESS.md.
    /// </summary>
    public class SwaggerAccessMiddleware
    {
        private readonly RequestDelegate _next;

        public SwaggerAccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            if (!context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            var credentials = configuration.GetSection("SwaggerAccess:Credentials").Get<List<SwaggerCredential>>()
                ?.Where(c => !string.IsNullOrEmpty(c.Username) && !string.IsNullOrEmpty(c.Password))
                .ToList() ?? new List<SwaggerCredential>();

            if (credentials.Count == 0)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            var allowedIps = configuration.GetSection("SwaggerAccess:AllowedIps").Get<string[]>()
                ?.Where(ip => !string.IsNullOrWhiteSpace(ip)).ToArray() ?? Array.Empty<string>();

            if (allowedIps.Length > 0)
            {
                var remoteIp = context.Connection.RemoteIpAddress;
                var ipAllowed = remoteIp != null && allowedIps.Any(ip =>
                    IPAddress.TryParse(ip, out var parsed) && parsed.Equals(remoteIp));

                if (!ipAllowed)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
            }

            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader["Basic ".Length..].Trim()));
                    var separatorIndex = decoded.IndexOf(':');

                    if (separatorIndex > 0)
                    {
                        var username = decoded[..separatorIndex];
                        var password = decoded[(separatorIndex + 1)..];

                        var isValid = credentials.Any(c =>
                            string.Equals(c.Username, username, StringComparison.Ordinal) &&
                            string.Equals(c.Password, password, StringComparison.Ordinal));

                        if (isValid)
                        {
                            await _next(context);
                            return;
                        }
                    }
                }
                catch (FormatException)
                {
                    // Malformed header, fall through to the 401 challenge below.
                }
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "Basic realm=\"Pastil API Docs\"";
        }
    }

    public static class SwaggerAccessMiddlewareExtensions
    {
        public static IApplicationBuilder UseSwaggerAccessControl(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SwaggerAccessMiddleware>();
        }
    }
}
