using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace Api.Authorization;

public sealed class AdminAreaRequirement : IAuthorizationRequirement { }

/// <summary>
/// Admin ⇒ همیشه مجاز. نقش دیگر ⇒ فقط اگر OnTokenValidatedService در همین درخواست مجوز RolePermission آن controller/action را تأیید
/// و PermissionVerifiedItemKey را در HttpContext.Items گذاشته باشد؛ در غیر این صورت رد (fail-closed).
/// </summary>
public sealed class AdminAreaAuthorizationHandler : AuthorizationHandler<AdminAreaRequirement>
{
    public const string PermissionVerifiedItemKey = "Pastil.AdminAreaPermissionVerified";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminAreaAuthorizationHandler(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminAreaRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        if (context.User.HasClaim("RoleId", ((long)Application.Common.Enumerable.RoleEnum.Admin).ToString()))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var items = _httpContextAccessor.HttpContext?.Items;
        if (items != null && items.TryGetValue(PermissionVerifiedItemKey, out var verified) && verified is true)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
