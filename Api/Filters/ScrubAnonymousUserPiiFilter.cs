using Application.Common.Privacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters;

/// <summary>
/// قاعده‌ی سراسری: endpointی که احراز هویت لازم ندارد (بدون [Authorize] یا با [AllowAnonymous]) هیچ‌وقت اطلاعات شخصی
/// «کاربران دیگر» (موبایل، ایمیل، کد معرف، موقعیت…) برنمی‌گرداند؛ حتی اگر خودِ درخواست‌دهنده تصادفاً توکن داشته باشد.
/// به‌جای اصلاح دستی صدها DTO، خروجی این endpointها قبل از سریال‌شدن پاک‌سازی می‌شود، پس endpoint عمومیِ جدید هم پوشش دارد.
/// </summary>
public sealed class ScrubAnonymousUserPiiFilter : IAsyncResultFilter
{
    // Account: پاسخ‌ها متعلق به خود درخواست‌دهنده است (ورود/بازیابی)؛ نباید دست‌کاری شود.
    private static readonly HashSet<string> ExemptControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Account"
    };

    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: not null } result
            && IsAnonymousEndpoint(context)
            && !IsExempt(context))
        {
            UserPiiScrubber.Scrub(result.Value);
        }

        return next();
    }

    private static bool IsAnonymousEndpoint(ResultExecutingContext context)
    {
        var metadata = context.ActionDescriptor.EndpointMetadata;
        return metadata.OfType<IAllowAnonymous>().Any() || !metadata.OfType<IAuthorizeData>().Any();
    }

    private static bool IsExempt(ResultExecutingContext context) =>
        context.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor descriptor
        && ExemptControllers.Contains(descriptor.ControllerName);
}
