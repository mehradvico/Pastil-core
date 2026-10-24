using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionPanelAccessSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters;

/// <summary>
/// ورود به «پنل/اپ نماینده»: همه‌ی endpointهای ناحیه‌ی Companion فقط برای ارائه‌دهنده، مالک یک مرکز یا عضو فعال تیم یک مرکز باز است.
/// کاربر عادی (مشتری) که هیچ‌کدام را ندارد 403 می‌گیرد؛ ادمین مستثناست. endpointهای AllowAnonymous و درخواست‌های احراز‌نشده
/// به همان [Authorize] سپرده می‌شوند. وضعیت ورود را بدون خطا با GET api/EndUser/CompanionPanelAccess بپرسید.
/// </summary>
public sealed class CompanionAreaAccessFilter : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.ActionDescriptor.RouteValues.TryGetValue("area", out var area) ||
            !string.Equals(area, "Companion", StringComparison.OrdinalIgnoreCase))
            return;
        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
            return;
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            return;

        var services = context.HttpContext.RequestServices;
        var current = (services.GetService(typeof(ICurrentUserHelper)) as ICurrentUserHelper)?.CurrentUser;
        if (current == null)
            return;
        if (string.Equals(current.RoleEnum, RoleEnum.Admin.ToString(), StringComparison.Ordinal))
            return;

        var access = services.GetService(typeof(ICompanionPanelAccessService)) as ICompanionPanelAccessService;
        if (access != null && await access.CanEnterAsync(current.UserId))
            return;

        context.Result = new ObjectResult(new BaseResultDto(false, Resource.Notification.AccessDenied)) { StatusCode = StatusCodes.Status403Forbidden };
    }
}
