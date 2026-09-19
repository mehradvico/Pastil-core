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
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers.Where(IsAdminController))
        {
            if (HasAllowAnonymous(controller))
            {
                throw new InvalidOperationException(
                    $"Admin controller '{controller.ControllerType.FullName}' cannot allow anonymous access.");
            }

            controller.Filters.Add(new AuthorizeFilter(PolicyNames.AdminOnly));
        }
    }

    private static bool IsAdminController(ControllerModel controller) =>
        controller.RouteValues.TryGetValue("area", out var area) &&
        string.Equals(area, "Admin", StringComparison.OrdinalIgnoreCase);

    private static bool HasAllowAnonymous(ControllerModel controller) =>
        controller.Attributes.OfType<IAllowAnonymous>().Any() ||
        controller.Actions.Any(action => action.Attributes.OfType<IAllowAnonymous>().Any());
}
