using Api.Authorization;
using Application.Common.CurrentUser;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Common.Security;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.CompanionSrvs.CompanionPanelAccessSrv;
using Application.Services.ProviderSrvs.ProviderUserSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;

namespace Api.Filters;

/// <summary>
/// دروازه‌ی پنل ارائه‌دهنده (ناحیه‌های Companion و Seller). سه نوع کاربر:
/// <list type="number">
/// <item>مالک (ارائه‌دهنده یا مالک مرکز) بدون هدر: مثل قبل با هویت خودش؛ چک مالکیت هر endpoint داده را محدود می‌کند.</item>
/// <item>کارمند پنل (ProviderUser) با هدر <c>X-Provider-Id</c>: عضویت فعال + کلید دسترسیِ همین endpoint (<see cref="ProviderEndpointPolicy"/>) چک می‌شود؛
/// اگر قبول شد، هویت همین یک درخواست با مالک ارائه‌دهنده جایگزین می‌شود (ActorUserId = خود کارمند) تا همان چک‌های مالکیتِ موجود
/// داده را به ارائه‌دهنده‌ی خودش محدود کنند. endpointهای OwnerOnly (ساخت نقش/دسترسی، کارمند، مالی سطح مالک) همیشه رد می‌شوند.</item>
/// <item>پرسنل/اپراتور مرکز (ClinicUser/GroomerUser/CoachUser) بدون هدر: فقط endpointهای شخصی/اپراتوری؛ پنل اپراتور خودش در ناحیه‌ی Operator است.</item>
/// </list>
/// endpointهای AllowAnonymous و درخواست‌های احراز‌نشده به [Authorize] سپرده می‌شوند؛ ادمین مستثناست (هدر برایش اثری ندارد).
/// </summary>
public sealed class ProviderPanelAccessFilter : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.ActionDescriptor.RouteValues.TryGetValue("area", out var area) || !ProviderEndpointPolicy.IsProviderArea(area))
            return;
        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
            return;
        var http = context.HttpContext;
        if (http.User.Identity?.IsAuthenticated != true)
            return;

        var services = http.RequestServices;
        var current = (services.GetService(typeof(ICurrentUserHelper)) as ICurrentUserHelper)?.CurrentUser;
        if (current == null)
            return;
        if (string.Equals(current.RoleEnum, RoleEnum.Admin.ToString(), StringComparison.Ordinal))
            return;

        context.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller);
        context.ActionDescriptor.RouteValues.TryGetValue("action", out var action);
        var rule = ProviderEndpointPolicy.Resolve(area, controller, action, http.Request.Method);

        var hasHeader = http.Request.Headers.TryGetValue(ProviderActingContext.HeaderName, out var headerValue) && !string.IsNullOrWhiteSpace(headerValue);
        if (hasHeader && rule.Mode != ProviderEndpointMode.Personal)
        {
            await ActAsProviderAsync(context, services, current, rule, headerValue.ToString(), area, controller, action);
            return;
        }

        // بدون هدر (یا endpoint شخصی): رفتار ناحیه‌ی Seller همان قاعده‌ی عضویت فروشگاه است (AreaMembership)
        if (!string.Equals(area, "Companion", StringComparison.OrdinalIgnoreCase))
            return;

        var access = services.GetService(typeof(ICompanionPanelAccessService)) as ICompanionPanelAccessService;
        var entry = access == null ? CompanionPanelEntry.None : await access.EntryAsync(current.UserId);
        if (entry == CompanionPanelEntry.Owner)
            return;
        if (entry == CompanionPanelEntry.StaffOnly &&
            (rule.StaffAllowed || rule.Mode == ProviderEndpointMode.Personal || rule.Mode == ProviderEndpointMode.Lookup))
            return;

        Deny(context, Resource.Notification.AccessDenied);
    }

    private static async Task ActAsProviderAsync(AuthorizationFilterContext context, IServiceProvider services, Application.Services.Dto.CurrentUserDto current,
        ProviderEndpointRule rule, string headerValue, string area, string controller, string action)
    {
        var audit = services.GetService(typeof(ISecurityAudit)) as ISecurityAudit;
        var http = context.HttpContext;

        if (!long.TryParse(headerValue.Trim(), out var providerId) || providerId <= 0)
        {
            Deny(context, Resource.Notification.ProviderActingAccessDenied);
            return;
        }

        if (rule.Mode == ProviderEndpointMode.OwnerOnly)
        {
            audit?.Observed("ProviderStaffDenied", current.UserId, subject: providerId.ToString(), detail: $"owner-only:{area}/{controller}/{action}");
            Deny(context, Resource.Notification.ProviderOwnerOnlyAction);
            return;
        }

        var staffService = services.GetService(typeof(IProviderUserService)) as IProviderUserService;
        var membership = staffService == null ? null : await staffService.ResolveActingAsync(current.UserId, providerId);
        if (membership == null)
        {
            audit?.Observed("ProviderStaffDenied", current.UserId, subject: providerId.ToString(), detail: $"not-member:{area}/{controller}/{action}");
            Deny(context, Resource.Notification.ProviderActingAccessDenied);
            return;
        }

        if (rule.Mode == ProviderEndpointMode.Permission && !ProviderPermissionCatalog.Grants(membership.Permissions, rule.Permission))
        {
            audit?.Observed("ProviderStaffDenied", current.UserId, subject: providerId.ToString(), detail: $"missing:{rule.Permission}:{area}/{controller}/{action}");
            Deny(context, Resource.Notification.YouHaveNotPermission);
            return;
        }

        var userService = services.GetService(typeof(IUserService)) as IUserService;
        var owner = userService == null ? null : await userService.GetCurrentUserByIdAsync(membership.OwnerUserId);
        if (owner == null)
        {
            Deny(context, Resource.Notification.ProviderActingAccessDenied);
            return;
        }

        // فروشگاه این درخواست: فروشگاهِ همین ارائه‌دهنده (نه هر فروشگاهی که مالک در Store.Users دارد)
        if (string.Equals(area, "Seller", StringComparison.OrdinalIgnoreCase))
        {
            owner.StoreId = await staffService.ResolveStoreIdAsync(membership.ProviderId, membership.OwnerUserId) ?? 0;
        }

        // نقش سیستمیِ درخواست همیشه نقش خودِ کارمند است، نه مالک: اگر مالک ادمین باشد، کارمند نباید از راه جانشینی میان‌برهای ادمین را بگیرد
        owner.RoleId = current.RoleId;
        owner.RoleName = current.RoleName;
        owner.RoleEnum = current.RoleEnum;
        owner.ActingProviderId = membership.ProviderId;
        owner.ActingProviderUserId = membership.ProviderUserId;
        owner.ActorUserId = current.UserId;
        ProviderActingContext.Set(http, owner);

        if (!HttpMethods.IsGet(http.Request.Method) && !HttpMethods.IsHead(http.Request.Method))
            audit?.Success("ProviderStaffAction", current.UserId, subject: membership.ProviderId.ToString(),
                detail: $"{http.Request.Method}:{area}/{controller}/{action}:{rule.Permission ?? rule.Mode.ToString()}");
    }

    private static void Deny(AuthorizationFilterContext context, string message) =>
        context.Result = new ObjectResult(new BaseResultDto(false, message)) { StatusCode = StatusCodes.Status403Forbidden };
}
