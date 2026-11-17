using Application.Common.Security;
using Hangfire.Dashboard;

namespace Api.HangFire;

/// <summary>
/// Hangfire's dashboard is operationally powerful (it exposes jobs and allows retries/deletes),
/// so it must not rely on Hangfire's development-only local-request default.
/// فقط «ادمین اصلی» (SuperAdminIdentity): نشانه‌ی تأییدشده‌ی OnTokenValidatedService روی همین درخواست؛ نقش «مدیر» کافی نیست.
/// </summary>
public sealed class AdminDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        return http.User.Identity?.IsAuthenticated == true && SuperAdminIdentity.IsVerified(http);
    }
}
