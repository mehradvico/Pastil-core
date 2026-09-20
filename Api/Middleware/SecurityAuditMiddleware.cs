using Application.Common.Security;

namespace Api.Middleware
{
    /// <summary>
    /// ثبت رویدادهای امنیتی HTTP در category <c>Security.Audit</c>:
    /// - 403/429 (دسترسی ممنوع، محدودیت نرخ) همیشه Warning؛ 401 فقط وقتی توکنی فرستاده شده (Information، چون انقضای عادی توکن هم می‌آید)
    /// - هر نوشتن موفق در ناحیه‌ی Admin (چه کسی، چه مسیری) به‌عنوان رد ممیزی ادمین
    /// فقط متد/مسیر/وضعیت/کاربر/IP ثبت می‌شود؛ query string و بدنه هرگز لاگ نمی‌شود.
    /// </summary>
    public sealed class SecurityAuditMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityAuditMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, ISecurityAudit audit)
        {
            await _next(context);

            try
            {
                var status = context.Response.StatusCode;
                long? userId = long.TryParse(context.User?.FindFirst("UserId")?.Value, out var id) ? id : null;
                var method = context.Request.Method;

                if (status == StatusCodes.Status403Forbidden || status == StatusCodes.Status429TooManyRequests)
                {
                    audit.Failure("Http" + status, userId, detail: "denied");
                }
                else if (status == StatusCodes.Status401Unauthorized && context.Request.Headers.ContainsKey("Authorization"))
                {
                    audit.Observed("Http401", userId, detail: "invalid_or_expired_token");
                }
                else if (status < 400 &&
                         !HttpMethods.IsGet(method) && !HttpMethods.IsHead(method) && !HttpMethods.IsOptions(method) &&
                         context.Request.Path.StartsWithSegments("/api/Admin", StringComparison.OrdinalIgnoreCase))
                {
                    audit.Success("AdminWrite", userId, detail: method);
                }
            }
            catch
            {
                // ثبت لاگ نباید پاسخ را بشکند
            }
        }
    }
}
