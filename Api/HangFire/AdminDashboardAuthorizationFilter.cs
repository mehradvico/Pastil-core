using Application.Common.Enumerable;
using Hangfire.Dashboard;

namespace Api.HangFire;

/// <summary>
/// Hangfire's dashboard is operationally powerful (it exposes jobs and allows retries/deletes),
/// so it must not rely on Hangfire's development-only local-request default.
/// </summary>
public sealed class AdminDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var user = context.GetHttpContext().User;
        return user.Identity?.IsAuthenticated == true &&
               user.HasClaim("RoleId", ((long)RoleEnum.Admin).ToString());
    }
}
