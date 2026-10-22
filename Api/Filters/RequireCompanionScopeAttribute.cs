using Application.Common.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters;

/// <summary>
/// endpointهای ناحیه‌ی Companion که با «شناسه‌ی نمایندگیِ کاربر» scope می‌شوند (مثل مدیریت پانسیون): اگر کاربر لاگین‌شده نمایندگی نداشته باشد،
/// CompanionId تهی است و فیلتر مالکیت سرویس‌ها اعمال نمی‌شود (یعنی همه‌ی ردیف‌ها دیده می‌شد). این فیلتر قبل از اجرا با 403 جلوی آن را می‌گیرد.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireCompanionScopeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.RequestServices.GetService(typeof(ICurrentUserHelper)) as ICurrentUserHelper;
        if (user?.CurrentUser?.CompanionId is not > 0)
            context.Result = new ForbidResult();
    }
}
