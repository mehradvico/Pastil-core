using Api.Filters;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionPanelAccessSrv;
using Application.Services.Dto;
using Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Xunit;

namespace Application.Tests.Coach;

/// <summary>ورود به پنل/اپ نماینده فقط برای ارائه‌دهنده، مالک مرکز یا عضو فعال تیم.</summary>
public class CompanionPanelAccessTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public CompanionPanelAccessTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private CompanionPanelAccessService NewService() =>
        new(_fixture.NewContext(), new MemoryCache(new MemoryCacheOptions { SizeLimit = 100 }));

    [Fact]
    public async Task Access_IsGrantedByProviderOwnershipOrActiveStaff_NotByBeingACustomer()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed(); // owner = ارائه‌دهنده؛ other و liker = کاربر عادی
        var service = NewService();

        var provider = await service.GetAsync(seed.OwnerUserId);
        Assert.True(provider.CanEnter);
        Assert.True(provider.IsProvider);

        // مشتری معمولی: هیچ‌چیز ندارد
        var customer = await service.GetAsync(seed.OtherUserId);
        Assert.False(customer.CanEnter);
        Assert.False(await service.CanEnterAsync(seed.OtherUserId));

        // مالک یک کلینیک (حتی «در حال بررسی») که ارائه‌دهنده نیست
        using (var ctx = _fixture.NewContext())
        {
            ctx.Clinics.Add(new Entities.Entities.ClinicField.Clinic { Name = "C", OwnerId = seed.OtherUserId, Phone = "1", AddressValue = "a", StateId = seed.StateId, CityId = seed.CityId, Active = false, Approved = false });
            await ctx.SaveChangesAsync();
        }
        var owner = await NewService().GetAsync(seed.OtherUserId);
        Assert.True(owner.CanEnter);
        Assert.True(owner.OwnsCenter);
        Assert.False(owner.IsProvider);

        // عضو تیم: دعوت‌نامه‌ی پذیرفته‌نشده کافی نیست، پذیرفته‌شده‌ی فعال کافی است، غیرفعال‌شده دوباره بسته است
        using (var ctx = _fixture.NewContext())
        {
            var clinicId = await ctx.Clinics.Where(c => c.OwnerId == seed.OtherUserId).Select(c => c.Id).FirstAsync();
            ctx.ClinicUsers.Add(new Entities.Entities.ClinicField.ClinicUser { ClinicId = clinicId, UserId = seed.LikerUserId, Active = true, UserAccept = null });
            await ctx.SaveChangesAsync();
        }
        Assert.False((await NewService().GetAsync(seed.LikerUserId)).CanEnter);
        using (var ctx = _fixture.NewContext())
            await ctx.ClinicUsers.Where(u => u.UserId == seed.LikerUserId).ExecuteUpdateAsync(s => s.SetProperty(u => u.UserAccept, (bool?)true));
        var staff = await NewService().GetAsync(seed.LikerUserId);
        Assert.True(staff.CanEnter);
        Assert.True(staff.IsStaff);
        using (var ctx = _fixture.NewContext())
            await ctx.ClinicUsers.Where(u => u.UserId == seed.LikerUserId).ExecuteUpdateAsync(s => s.SetProperty(u => u.Active, false));
        Assert.False((await NewService().GetAsync(seed.LikerUserId)).CanEnter);
    }

    private static async Task<IActionResult?> RunFilterAsync(string area, string roleEnum, bool canEnter, bool authenticated = true, bool allowAnonymous = false)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICurrentUserHelper>(FakeProxy<ICurrentUserHelper>.Create((m, _) =>
            m.Name == "get_CurrentUser" ? new CurrentUserDto { UserId = 7, RoleEnum = roleEnum } : null).Instance);
        services.AddSingleton<ICompanionPanelAccessService>(FakeProxy<ICompanionPanelAccessService>.Create((m, _) =>
            m.Name == nameof(ICompanionPanelAccessService.CanEnterAsync) ? Task.FromResult(canEnter) : null).Instance);
        var http = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        if (authenticated)
            http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("UserId", "7") }, "test"));

        var descriptor = new ActionDescriptor { EndpointMetadata = allowAnonymous ? new List<object> { new AllowAnonymousAttribute() } : new List<object>() };
        descriptor.RouteValues["area"] = area;
        var context = new AuthorizationFilterContext(new ActionContext(http, new RouteData(), descriptor), new List<IFilterMetadata>());
        await new CompanionAreaAccessFilter().OnAuthorizationAsync(context);
        return context.Result;
    }

    [Fact]
    public async Task Filter_BlocksOnlyAuthenticatedCustomersInTheCompanionArea()
    {
        // مشتری بدون دسترسی: 403
        var denied = Assert.IsType<ObjectResult>(await RunFilterAsync("Companion", "Customer", canEnter: false));
        Assert.Equal(403, denied.StatusCode);

        // با دسترسی، ادمین، ناحیه‌ی دیگر، AllowAnonymous و درخواست بدون ورود: عبور
        Assert.Null(await RunFilterAsync("Companion", "Customer", canEnter: true));
        Assert.Null(await RunFilterAsync("Companion", "Admin", canEnter: false));
        Assert.Null(await RunFilterAsync("EndUser", "Customer", canEnter: false));
        Assert.Null(await RunFilterAsync("Companion", "Customer", canEnter: false, allowAnonymous: true));
        Assert.Null(await RunFilterAsync("Companion", "Customer", canEnter: false, authenticated: false));
    }
}
