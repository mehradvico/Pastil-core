using System.Threading.Tasks;
using Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Api.Authorization;

public sealed class SuperAdminRequirement : IAuthorizationRequirement { }

/// <summary>
/// فقط «ادمین اصلی»: نشانه‌ی SuperAdminIdentity.VerifiedItemKey را فقط OnTokenValidatedService بعد از تطبیق با رکورد دیتابیس و
/// پیکربندی سرور روی همین درخواست می‌گذارد. هیچ claim توکن یا نقشی (حتی «مدیر») جایگزین آن نمی‌شود (fail-closed).
/// </summary>
public sealed class SuperAdminAuthorizationHandler : AuthorizationHandler<SuperAdminRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SuperAdminAuthorizationHandler(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SuperAdminRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true && SuperAdminIdentity.IsVerified(_httpContextAccessor))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
