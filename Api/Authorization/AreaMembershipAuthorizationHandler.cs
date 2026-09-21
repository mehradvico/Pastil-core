using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Application.Common.Interface;
using Application.Common.Security;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;

namespace Api.Authorization;

public sealed class AreaMembershipRequirement : IAuthorizationRequirement
{
    public AreaMembershipRequirement(string area) => Area = area;
    public string Area { get; }
}

public static class AreaMembership
{
    /// <summary>کاربر عضو ناحیه است؟ Seller = فروشگاه، Companion = مرکز (مالک یا عضو تأییدشده)، Driver = راننده. Admin همیشه عضو حساب می‌شود.</summary>
    public static bool IsMember(string area, CurrentUserDto user)
    {
        if (user == null)
            return false;

        if (user.RoleId == (long)Application.Common.Enumerable.RoleEnum.Admin)
            return true;

        return area?.ToLowerInvariant() switch
        {
            "seller" => user.StoreId > 0,
            "companion" => (user.CompanionId ?? 0) > 0 || user.IsCompanionUser,
            "driver" => user.DriverId > 0,
            _ => true
        };
    }
}

/// <summary>
/// سیاست عضویت ناحیه‌های Seller/Companion/Driver. حالت پیش‌فرض Audit است: غیرعضو رد نمی‌شود ولی رویداد AreaNonMember در
/// Security.Audit ثبت می‌شود (تا جریان‌های قبل از عضویت که ممکن است این ناحیه‌ها را صدا بزنند شناسایی شوند). بعد از یک دوره بدون رویداد
/// مشروع، با Security:AreaPolicyMode=Enforce (env PASTIL_AREA_POLICY_MODE) غیرعضو رد می‌شود.
/// </summary>
public sealed class AreaMembershipAuthorizationHandler : AuthorizationHandler<AreaMembershipRequirement>
{
    private static readonly ConcurrentDictionary<string, DateTime> LastAudit = new();

    private readonly ICurrentUserHelper _currentUser;
    private readonly ISecurityAudit _audit;
    private readonly IConfiguration _configuration;

    public AreaMembershipAuthorizationHandler(ICurrentUserHelper currentUser, ISecurityAudit audit, IConfiguration configuration)
    {
        _currentUser = currentUser;
        _audit = audit;
        _configuration = configuration;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AreaMembershipRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        var user = _currentUser.CurrentUser;
        if (AreaMembership.IsMember(requirement.Area, user))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var enforce = string.Equals(_configuration["Security:AreaPolicyMode"], "Enforce", StringComparison.OrdinalIgnoreCase);
        var key = requirement.Area + ":" + user?.UserId;
        if (!LastAudit.TryGetValue(key, out var last) || DateTime.UtcNow - last > TimeSpan.FromMinutes(10))
        {
            LastAudit[key] = DateTime.UtcNow;
            if (LastAudit.Count > 20_000)
                LastAudit.Clear();
            _audit.Observed("AreaNonMember", user?.UserId, detail: $"{requirement.Area}:{(enforce ? "denied" : "allowed_audit_mode")}");
        }

        if (!enforce)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
