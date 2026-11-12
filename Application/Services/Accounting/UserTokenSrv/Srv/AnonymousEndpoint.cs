using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Accounting.UserTokenSrv.Srv
{
    /// <summary>
    /// endpointی که ورود لازم ندارد ([AllowAnonymous] یا بدون [Authorize]).
    /// توکن نامعتبر/منقضی/باطل روی چنین endpointی نباید پاسخ ۴۰۱ بنویسد: درخواست مثل مهمان ادامه پیدا می‌کند.
    /// نوشتن ۴۰۱ در رویدادهای JwtBearer و ادامه‌ی pipeline باعث می‌شد controller بعد از شروع پاسخ دوباره بنویسد
    /// و اتصال قطع شود (وب‌اپ: ۵۰۲ و «دریافت فهرست … ناموفق بود» / «صفحه پیدا نشد» برای کاربر با کوکی توکن کهنه).
    /// endpointهای محافظت‌شده همان رفتار قبلی را دارند (۴۰۱/۴۰۳ با پیام).
    /// </summary>
    internal static class AnonymousEndpoint
    {
        public static bool Allows(HttpContext httpContext)
        {
            var metadata = httpContext?.GetEndpoint()?.Metadata;
            if (metadata == null) return false; // مسیر نامشخص: رفتار قبلی (fail-closed)
            if (metadata.GetMetadata<IAllowAnonymous>() != null) return true;
            return metadata.GetMetadata<IAuthorizeData>() == null;
        }
    }
}
