using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Api.Authorization;

/// <summary>
/// Makes the API's Admin area deny every non-admin user by default.
/// Controller attributes are easy to omit on a newly added endpoint, so this
/// convention is deliberately centralized instead of relying on each Admin
/// controller to remember the policy.
/// </summary>
public sealed class AdminAreaAuthorizationConvention : IApplicationModelConvention
{
    /// <summary>
    /// کنترلرهای Admin که هیچ‌وقت به نقش تفویض‌شده داده نمی‌شوند (ارتقای دسترسی، پول، تنظیمات سیستمی، زیرساخت). بقیه‌ی ناحیه‌ی Admin
    /// با policy «AdminArea» (Admin یا مجوز RolePermission همان action) محافظت می‌شود.
    /// </summary>
    public static readonly IReadOnlySet<string> AdminOnlyControllers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Role", "RolePermission", "Permission", "PermissionSync", "User",
        "Merchant", "Wallet", "Code", "CodeGroup", "BaseDetail",
        "ServerMonitoring", "Notice", "PushBroadcast", "PushDiagnostics", "MissingProduct"
    };

    private static readonly string[] MembershipAreas = { "Seller", "Companion", "Driver" };

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers.Where(IsAdminController))
        {
            if (HasAllowAnonymous(controller))
            {
                throw new InvalidOperationException(
                    $"Admin controller '{controller.ControllerType.FullName}' cannot allow anonymous access.");
            }

            var strict = AdminOnlyControllers.Contains(ControllerName(controller));
            controller.Filters.Add(new AuthorizeFilter(strict ? PolicyNames.AdminOnly : PolicyNames.AdminArea));
        }

        foreach (var controller in application.Controllers)
        {
            if (controller.RouteValues.TryGetValue("area", out var area) &&
                MembershipAreas.Contains(area, StringComparer.OrdinalIgnoreCase))
            {
                controller.Filters.Add(new AuthorizeFilter(PolicyNames.AreaMember(area)));
            }
        }
    }

    private static string ControllerName(ControllerModel controller) =>
        controller.RouteValues.TryGetValue("controller", out var name) && !string.IsNullOrEmpty(name)
            ? name
            : controller.ControllerName;

    private static bool IsAdminController(ControllerModel controller) =>
        controller.RouteValues.TryGetValue("area", out var area) &&
        string.Equals(area, "Admin", StringComparison.OrdinalIgnoreCase);

    private static bool HasAllowAnonymous(ControllerModel controller) =>
        controller.Attributes.OfType<IAllowAnonymous>().Any() ||
        controller.Actions.Any(action => action.Attributes.OfType<IAllowAnonymous>().Any());
}
