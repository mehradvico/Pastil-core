using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Security
{
    /// <summary>
    /// هدرهای امنیتی پایه برای سرویس‌های بک‌اند (API/File/Payment). قبلاً هیچ‌کدام تنظیم نمی‌شد:
    /// نه جلوی MIME sniffing، نه clickjacking صفحه‌های Swagger/پرداخت، نه نشت Referer.
    /// Cache-Control فقط برای درخواست‌های لاگین‌شده عوض می‌شود: پاسخِ مخصوص یک کاربر نباید در CDN/پروکسی مشترک کش شود
    /// (وگرنه پروفایل یک کاربر به همه نشان داده می‌شود). پاسخ‌های عمومی (بدون Authorization) مثل قبل به CDN سپرده می‌شوند.
    /// </summary>
    public static class SecurityHeadersExtensions
    {
        public static IApplicationBuilder UseBackendSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                context.Response.OnStarting(() =>
                {
                    ApplyHeaders(context);
                    return System.Threading.Tasks.Task.CompletedTask;
                });
                await next();
            });
        }

        public static void ApplyHeaders(HttpContext context)
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";

            if (context.Request.Headers.ContainsKey("Authorization") || context.User?.Identity?.IsAuthenticated == true)
            {
                headers.CacheControl = "private, no-store";
                if (!headers.Vary.ToString().Contains("Authorization", System.StringComparison.OrdinalIgnoreCase))
                    headers.Append("Vary", "Authorization");
            }
        }
    }
}
