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

    private static async Task<bool> RunAdminAreaAsync(ClaimsPrincipal user, bool permissionVerified)
    {
        var http = new DefaultHttpContext();
        if (permissionVerified)
            http.Items[AdminAreaAuthorizationHandler.PermissionVerifiedItemKey] = true;
        var handler = new AdminAreaAuthorizationHandler(new HttpContextAccessor { HttpContext = http });
        var requirement = new AdminAreaRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await handler.HandleAsync(context);
        return context.HasSucceeded;
    }

    [Fact]
    public async Task Admin_role_always_passes_the_admin_area_policy()
    {
        Assert.True(await RunAdminAreaAsync(User(Admin), permissionVerified: false));
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
    [InlineData("User")]
    [InlineData("Merchant")]
    [InlineData("Wallet")]
    [InlineData("ServerMonitoring")]
    public void Privilege_and_money_controllers_stay_admin_only(string controllerName)
    {
        var model = new ControllerModel(typeof(AreaAuthorizationTests).GetTypeInfo(), new List<object>());
        model.RouteValues["area"] = "Admin";
        model.RouteValues["controller"] = controllerName;
        var application = new ApplicationModel();
        application.Controllers.Add(model);

        new AdminAreaAuthorizationConvention().Apply(application);

        var filter = Assert.Single(model.Filters.OfType<AuthorizeFilter>());
        Assert.Equal(PolicyNames.AdminOnly, Assert.Single(filter.AuthorizeData).Policy);
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
