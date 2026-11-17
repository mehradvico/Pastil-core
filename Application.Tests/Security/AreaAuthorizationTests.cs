using System.Reflection;
using System.Security.Claims;
using Api.Authorization;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Xunit;

namespace Application.Tests.Security;

public class AreaAuthorizationTests
{
    private const long Admin = (long)Application.Common.Enumerable.RoleEnum.Admin;

    private static ClaimsPrincipal User(long roleId) =>
        new(new ClaimsIdentity(new[] { new Claim("UserId", "7"), new Claim("RoleId", roleId.ToString()) }, "test"));

    private static async Task<bool> RunAdminAreaAsync(ClaimsPrincipal user, bool permissionVerified, bool superAdminVerified = false)
    {
        var http = new DefaultHttpContext();
        if (permissionVerified)
            http.Items[AdminAreaAuthorizationHandler.PermissionVerifiedItemKey] = true;
        if (superAdminVerified)
            http.Items[Application.Common.Security.SuperAdminIdentity.VerifiedItemKey] = true;
        var handler = new AdminAreaAuthorizationHandler(new HttpContextAccessor { HttpContext = http });
        var requirement = new AdminAreaRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await handler.HandleAsync(context);
        return context.HasSucceeded;
    }

    [Fact]
    public async Task Super_admin_always_passes_the_admin_area_policy_without_any_permission()
    {
        Assert.True(await RunAdminAreaAsync(User(Admin), permissionVerified: false, superAdminVerified: true));
    }

    [Fact]
    public async Task Manager_role_claim_alone_no_longer_opens_the_admin_area_policy()
    {
        // نقش «مدیر» (RoleId = 2) بدون هویت ادمین اصلی مثل هر نقش دیگر تابع RolePermission است
        Assert.False(await RunAdminAreaAsync(User(Admin), permissionVerified: false));
        Assert.True(await RunAdminAreaAsync(User(Admin), permissionVerified: true));
    }

    [Fact]
    public async Task Forged_role_claim_cannot_make_anyone_super_admin()
    {
        // فقط نشانه‌ی HttpContext.Items (که سرور بعد از تطبیق با دیتابیس می‌گذارد) ادمین اصلی می‌سازد، نه claim توکن
        var handler = new SuperAdminAuthorizationHandler(new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
        var context = new AuthorizationHandlerContext(new[] { new SuperAdminRequirement() }, User(Admin), null);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Verified_super_admin_passes_the_super_admin_only_policy()
    {
        var http = new DefaultHttpContext();
        http.Items[Application.Common.Security.SuperAdminIdentity.VerifiedItemKey] = true;
        var handler = new SuperAdminAuthorizationHandler(new HttpContextAccessor { HttpContext = http });
        var context = new AuthorizationHandlerContext(new[] { new SuperAdminRequirement() }, User(Admin), null);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task Delegated_role_passes_only_when_its_permission_was_verified_for_this_request()
    {
        Assert.True(await RunAdminAreaAsync(User(5), permissionVerified: true));
        Assert.False(await RunAdminAreaAsync(User(5), permissionVerified: false));
    }

    [Fact]
    public async Task Anonymous_user_never_passes_the_admin_area_policy()
    {
        Assert.False(await RunAdminAreaAsync(new ClaimsPrincipal(new ClaimsIdentity()), permissionVerified: true));
    }

    [Theory]
    [InlineData("Seller", 0, null, false, 0, false)]
    [InlineData("Seller", 12, null, false, 0, true)]
    [InlineData("Companion", 0, null, false, 0, false)]
    [InlineData("Companion", 0, 9, false, 0, true)]
    [InlineData("Companion", 0, null, true, 0, true)]
    [InlineData("Driver", 0, null, false, 0, false)]
    [InlineData("Driver", 0, null, false, 3, true)]
    [InlineData("EndUser", 0, null, false, 0, true)]
    public void Area_membership_follows_store_companion_and_driver_ownership(string area, int storeId, int? companionId, bool companionUser, int driverId, bool expected)
    {
        var user = new CurrentUserDto { UserId = 7, RoleId = 5, StoreId = storeId, CompanionId = companionId, IsCompanionUser = companionUser, DriverId = driverId };

        Assert.Equal(expected, AreaMembership.IsMember(area, user));
    }

    [Fact]
    public void Admin_counts_as_a_member_of_every_area_and_null_user_of_none()
    {
        Assert.True(AreaMembership.IsMember("Seller", new CurrentUserDto { RoleId = Admin }));
        Assert.False(AreaMembership.IsMember("Seller", null!));
    }

    [Theory]
    [InlineData("Role")]
    [InlineData("RolePermission")]
    [InlineData("Permission")]
    [InlineData("PermissionSync")]
    [InlineData("Merchant")]
    [InlineData("Wallet")]
    [InlineData("ServerMonitoring")]
    [InlineData("PushDiagnostics")]
    [InlineData("UserMemory")]
    [InlineData("PastilMatchMessage")]
    [InlineData("PastilMatchMessageAttachment")]
    [InlineData("PastilMatchMessageReaction")]
    public void Privilege_money_infra_and_confidential_controllers_are_super_admin_only(string controllerName)
    {
        var model = new ControllerModel(typeof(AreaAuthorizationTests).GetTypeInfo(), new List<object>());
        model.RouteValues["area"] = "Admin";
        model.RouteValues["controller"] = controllerName;
        var application = new ApplicationModel();
        application.Controllers.Add(model);

        new AdminAreaAuthorizationConvention().Apply(application);

        var filter = Assert.Single(model.Filters.OfType<AuthorizeFilter>());
        Assert.Equal(PolicyNames.SuperAdminOnly, Assert.Single(filter.AuthorizeData).Policy);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Code")]
    [InlineData("Notice")]
    [InlineData("Ticket")]
    public void Other_admin_controllers_follow_role_permissions(string controllerName)
    {
        var model = new ControllerModel(typeof(AreaAuthorizationTests).GetTypeInfo(), new List<object>());
        model.RouteValues["area"] = "Admin";
        model.RouteValues["controller"] = controllerName;
        var application = new ApplicationModel();
        application.Controllers.Add(model);

        new AdminAreaAuthorizationConvention().Apply(application);

        var filter = Assert.Single(model.Filters.OfType<AuthorizeFilter>());
        Assert.Equal(PolicyNames.AdminArea, Assert.Single(filter.AuthorizeData).Policy);
    }

    [Theory]
    [InlineData("Seller")]
    [InlineData("Companion")]
    [InlineData("Driver")]
    public void Seller_companion_and_driver_areas_get_the_membership_policy(string area)
    {
        var model = new ControllerModel(typeof(AreaAuthorizationTests).GetTypeInfo(), new List<object>());
        model.RouteValues["area"] = area;
        model.RouteValues["controller"] = "Anything";
        var application = new ApplicationModel();
        application.Controllers.Add(model);

        new AdminAreaAuthorizationConvention().Apply(application);

        var filter = Assert.Single(model.Filters.OfType<AuthorizeFilter>());
        Assert.Equal(PolicyNames.AreaMember(area), Assert.Single(filter.AuthorizeData).Policy);
    }
}
