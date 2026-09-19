using System.Reflection;
using Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Xunit;

namespace Application.Tests.Security;

public class AdminAreaAuthorizationConventionTests
{
    [Fact]
    public void Apply_adds_the_admin_policy_to_every_admin_controller()
    {
        var application = new ApplicationModel();
        var controller = CreateController<AdminEndpoint>("Admin");
        application.Controllers.Add(controller);

        new AdminAreaAuthorizationConvention().Apply(application);

        var filter = Assert.Single(controller.Filters.OfType<AuthorizeFilter>());
        Assert.Equal(PolicyNames.AdminOnly, Assert.Single(filter.AuthorizeData).Policy);
    }

    [Fact]
    public void Apply_does_not_change_non_admin_controllers()
    {
        var application = new ApplicationModel();
        var controller = CreateController<EndUserEndpoint>("EndUser");
        application.Controllers.Add(controller);

        new AdminAreaAuthorizationConvention().Apply(application);

        Assert.Empty(controller.Filters.OfType<AuthorizeFilter>());
    }

    [Fact]
    public void Apply_rejects_an_anonymous_admin_endpoint()
    {
        var application = new ApplicationModel();
        var controller = CreateController<AnonymousAdminEndpoint>("Admin");
        application.Controllers.Add(controller);

        var exception = Assert.Throws<InvalidOperationException>(
            () => new AdminAreaAuthorizationConvention().Apply(application));

        Assert.Contains("cannot allow anonymous", exception.Message);
    }

    private static ControllerModel CreateController<TController>(string area)
    {
        var controller = new ControllerModel(
            typeof(TController).GetTypeInfo(),
            typeof(TController)
                .GetCustomAttributes(inherit: true)
                .Cast<object>()
                .ToList());
        controller.RouteValues["area"] = area;
        return controller;
    }

    private sealed class AdminEndpoint : ControllerBase
    {
    }

    private sealed class EndUserEndpoint : ControllerBase
    {
    }

    [AllowAnonymous]
    private sealed class AnonymousAdminEndpoint : ControllerBase
    {
    }
}
