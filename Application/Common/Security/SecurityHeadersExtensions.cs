using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Security
{
    /// <summary>
    /// هدرهای امنیتی پایه برای سرویس‌های بک‌اند (API/File/Payment). قبلاً هیچ‌کدام تنظیم نمی‌شد:
    /// نه جلوی MIME sniffing، نه clickjacking صفحه‌های Swagger/پرداخت، نه نشت Referer.
    /// Cache-Control عمداً دست‌نخورده می‌ماند (Output Cache / CDN سیاست خودشان را دارند).
    /// </summary>
    public static class SecurityHeadersExtensions
    {
        public static IApplicationBuilder UseBackendSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                context.Response.OnStarting(() =>
                {
                    var headers = context.Response.Headers;
                    headers["X-Content-Type-Options"] = "nosniff";
                    headers["X-Frame-Options"] = "DENY";
                    headers["Referrer-Policy"] = "no-referrer";
                    headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";
                    return System.Threading.Tasks.Task.CompletedTask;
                });
                await next();
            });
        }
    }
}
