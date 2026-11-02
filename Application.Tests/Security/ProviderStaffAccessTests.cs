using Api.Authorization;
using Api.Filters;
using Application.Common.CurrentUser;
using Application.Common.Interface;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.CompanionSrvs.CompanionPanelAccessSrv;
using Application.Services.Dto;
using Application.Services.ProviderSrvs.ProviderUserSrv;
using Application.Services.ProviderSrvs.ProviderUserSrv.Dto;
using Application.Tests.Coach;
using Entities.Entities.ProviderField;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Xunit;
using Sec = Application.Services.ProviderSrvs.ProviderUserSrv.ProviderPermissionCatalog.Sections;

namespace Application.Tests.Security;

/// <summary>نقش و دسترسی کارمندان پنل ارائه‌دهنده (ProviderUser): نگاشت endpointها، کاتالوگ، فیلتر جانشینی.</summary>
public class ProviderStaffAccessTests
{
    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Pastil.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Pastil.sln not found.");
    }

    private static readonly Regex HttpAttribute = new(@"\[Http(Get|Post|Put|Delete|Patch)\b[^\]]*\]\s*(?:\[[^\]]*\]\s*)*public\s+(?:async\s+)?[\w<>.]+(?:<[\w<>.]+>)?\s+(\w+)\s*\(", RegexOptions.Compiled);

    private static IEnumerable<(string Area, string Controller, string Action, string Method)> Endpoints()
    {
        foreach (var area in ProviderEndpointPolicy.Areas)
        {
            var dir = Path.Combine(RepoRoot(), "Api", "Areas", area, "Controllers");
            foreach (var file in Directory.GetFiles(dir, "*Controller.cs"))
            {
                var controller = Path.GetFileNameWithoutExtension(file)[..^"Controller".Length];
                foreach (Match match in HttpAttribute.Matches(File.ReadAllText(file)))
                    yield return (area, controller, match.Groups[2].Value, match.Groups[1].Value.ToUpperInvariant());
            }
        }
    }

    [Fact]
    public void Every_provider_panel_controller_has_an_explicit_rule()
    {
        var unmapped = Endpoints().Select(e => e.Area + "/" + e.Controller).Distinct()
            .Where(name => !ProviderEndpointPolicy.IsMapped(name.Split('/')[0], name.Split('/')[1]))
            .ToList();
        Assert.True(unmapped.Count == 0, "Add these controllers to ProviderEndpointPolicy: " + string.Join(", ", unmapped));
        Assert.True(Endpoints().Count() > 300); // اسکن واقعاً کنترلرها را دیده است
    }

    [Fact]
    public void Every_permission_rule_resolves_to_a_catalog_key_except_writes_on_view_only_sections()
    {
        foreach (var (area, controller, action, method) in Endpoints())
        {
            var rule = ProviderEndpointPolicy.Resolve(area, controller, action, method);
            if (rule.Mode != ProviderEndpointMode.Permission)
                continue;
            // حسابداری فقط «مشاهده» دارد؛ هر متد نوشتنی روی آن باید با کلید ناشناخته همیشه رد شود
            Assert.True(ProviderPermissionCatalog.IsKnown(rule.Permission) || rule.Permission.EndsWith(".Finance.Manage"),
                $"{area}/{controller}/{action} → {rule.Permission}");
        }
    }

    [Theory]
    [InlineData("Companion", "ProviderUserRole", "Post", "POST")]
    [InlineData("Companion", "ProviderUserRole", "Get", "GET")]
    [InlineData("Companion", "ProviderUser", "Post", "POST")]
    [InlineData("Companion", "ProviderUser", "Role", "PUT")]
    [InlineData("Companion", "ProviderPermission", "Get", "GET")]
    [InlineData("Companion", "Clinic", "Post", "POST")]
    [InlineData("Companion", "Clinic", "Delete", "DELETE")]
    [InlineData("Companion", "FinanceCompanion", "Get", "GET")]
    [InlineData("Companion", "CompanionUserGetMobile", "Get", "GET")]
    [InlineData("Companion", "SomeFutureController", "Get", "GET")]
    [InlineData("Seller", "SomeFutureController", "Post", "POST")]
    public void Owner_level_endpoints_are_never_open_to_panel_staff(string area, string controller, string action, string method)
    {
        Assert.Equal(ProviderEndpointMode.OwnerOnly, ProviderEndpointPolicy.Resolve(area, controller, action, method).Mode);
    }

    [Theory]
    [InlineData("Companion", "ClinicReserve", "Get", "GET", "Clinic.Reserve.View")]
    [InlineData("Companion", "ClinicReserve", "Approve", "PUT", "Clinic.Reserve.Manage")]
    [InlineData("Companion", "ClinicSettlement", "Get", "GET", "Clinic.Finance.View")]
    [InlineData("Companion", "Clinic", "Put", "PUT", "Clinic.Profile.Manage")]
    [InlineData("Companion", "GroomerPortfolio", "Post", "POST", "Groomer.Portfolio.Manage")]
    [InlineData("Companion", "SchoolTeaching", "Dashboard", "GET", "School.Finance.View")]
    [InlineData("Seller", "ProductOrder", "Get", "GET", "Store.Order.View")]
    [InlineData("Seller", "Settlement", "Get", "GET", "Store.Finance.View")]
    public void Venue_endpoints_map_to_their_mother_scope_section_and_level(string area, string controller, string action, string method, string key)
    {
        var rule = ProviderEndpointPolicy.Resolve(area, controller, action, method);
        Assert.Equal(ProviderEndpointMode.Permission, rule.Mode);
        Assert.Equal(key, rule.Permission);
    }

    [Fact]
    public void Catalog_grants_exact_keys_and_manage_implies_view_of_the_same_section_only()
    {
        var accountant = new HashSet<string> { "Clinic.Finance.View" };
        Assert.True(ProviderPermissionCatalog.Grants(accountant, "Clinic.Finance.View"));
        Assert.False(ProviderPermissionCatalog.Grants(accountant, "Clinic.Reserve.View"));
        Assert.False(ProviderPermissionCatalog.Grants(accountant, "Groomer.Finance.View"));
        Assert.False(ProviderPermissionCatalog.Grants(accountant, "Clinic.Finance.Manage"));

        var manager = new HashSet<string> { "Clinic.Reserve.Manage" };
        Assert.True(ProviderPermissionCatalog.Grants(manager, "Clinic.Reserve.View"));
        Assert.False(ProviderPermissionCatalog.Grants(manager, "Clinic.Service.View"));

        // کلید ناشناخته یا با حروف متفاوت هیچ‌وقت چیزی نمی‌دهد
        Assert.False(ProviderPermissionCatalog.Grants(new HashSet<string> { "clinic.finance.view" }, "Clinic.Finance.View"));
        Assert.False(ProviderPermissionCatalog.Grants(new HashSet<string> { "Clinic.*" }, "Clinic.Finance.View"));
        Assert.False(ProviderPermissionCatalog.IsKnown("Clinic.Finance.Manage"));
    }

    [Fact]
    public void Role_keys_outside_the_providers_venues_or_unknown_are_rejected_as_a_whole()
    {
        var scopes = new HashSet<string> { ProviderRoleLabels.Clinic };
        Assert.Equal(new[] { "Clinic.Finance.View", "Clinic.Reserve.View" },
            ProviderPermissionCatalog.Normalize(new[] { "Clinic.Reserve.View", " Clinic.Finance.View ", "Clinic.Reserve.View", "" }, scopes));
        Assert.Null(ProviderPermissionCatalog.Normalize(new[] { "Clinic.Reserve.View", "Store.Order.View" }, scopes));
        Assert.Null(ProviderPermissionCatalog.Normalize(new[] { "Clinic.Reserve.Delete" }, scopes));
    }

    // ------------------------------------------------------------------ فیلتر

    private const long StaffUserId = 50;
    private const long OwnerUserId = 7;
    private const long ProviderId = 3;

    private static async Task<(IActionResult? Result, HttpContext Http)> RunAsync(string area, string controller, string action, string method,
        string? header, ProviderActingAccess? membership, CompanionPanelEntry entry = CompanionPanelEntry.None)
    {
        var services = new ServiceCollection();
        var real = new CurrentUserDto { UserId = StaffUserId, RoleId = 4, RoleEnum = "Customer", RoleName = "c" };
        services.AddSingleton<ICurrentUserHelper>(FakeProxy<ICurrentUserHelper>.Create((m, _) => m.Name == "get_CurrentUser" ? real : null).Instance);
        services.AddSingleton<ICompanionPanelAccessService>(FakeProxy<ICompanionPanelAccessService>.Create((m, _) =>
            m.Name == nameof(ICompanionPanelAccessService.EntryAsync) ? Task.FromResult(entry) : null).Instance);
        services.AddSingleton<IProviderUserService>(FakeProxy<IProviderUserService>.Create((m, args) =>
            m.Name == nameof(IProviderUserService.ResolveActingAsync)
                ? Task.FromResult((long)args![1]! == ProviderId ? membership : null)
                : null).Instance);
        services.AddSingleton<IUserService>(FakeProxy<IUserService>.Create((m, args) =>
            m.Name == nameof(IUserService.GetCurrentUserByIdAsync)
                ? Task.FromResult<CurrentUserDto?>(new CurrentUserDto { UserId = (long)args![0]!, RoleId = 1, RoleEnum = "Admin", CompanionId = 9 })
                : null).Instance);

        var http = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("UserId", StaffUserId.ToString()) }, "test"));
        http.Request.Method = method;
        if (header != null)
            http.Request.Headers[ProviderActingContext.HeaderName] = header;

        var descriptor = new ActionDescriptor { EndpointMetadata = new List<object>() };
        descriptor.RouteValues["area"] = area;
        descriptor.RouteValues["controller"] = controller;
        descriptor.RouteValues["action"] = action;
        var context = new AuthorizationFilterContext(new ActionContext(http, new RouteData(), descriptor), new List<IFilterMetadata>());
        await new ProviderPanelAccessFilter().OnAuthorizationAsync(context);
        return (context.Result, http);
    }

    private static ProviderActingAccess Member(params string[] keys) => new()
    {
        ProviderId = ProviderId, ProviderUserId = 11, OwnerUserId = OwnerUserId, ProviderUserRoleId = 5,
        Permissions = new HashSet<string>(keys),
    };

    private static void AssertDenied(IActionResult? result) => Assert.Equal(403, Assert.IsType<ObjectResult>(result).StatusCode);

    [Fact]
    public async Task Staff_with_the_endpoint_permission_acts_as_the_owner_but_keeps_their_own_role_and_identity_trail()
    {
        var (result, http) = await RunAsync("Companion", "ClinicSettlement", "Get", "GET", "3", Member("Clinic.Finance.View"));

        Assert.Null(result);
        var acting = ProviderActingContext.Get(http);
        Assert.NotNull(acting);
        Assert.Equal(OwnerUserId, acting!.UserId);          // چک‌های مالکیت موجود → داده‌ی همین ارائه‌دهنده
        Assert.Equal(StaffUserId, acting.ActorUserId);      // رد کاربر واقعی
        Assert.Equal(ProviderId, acting.ActingProviderId);
        Assert.Equal("Customer", acting.RoleEnum);          // نقش ادمینِ مالک به کارمند نمی‌رسد
        Assert.Equal(4, acting.RoleId);
        Assert.True(acting.IsActingForProvider);
    }

    [Fact]
    public async Task Accountant_of_the_clinic_cannot_open_clinic_reserves_or_other_venues()
    {
        var accountant = Member("Clinic.Finance.View");
        AssertDenied((await RunAsync("Companion", "ClinicReserve", "Get", "GET", "3", accountant)).Result);
        AssertDenied((await RunAsync("Companion", "GroomerSettlement", "Get", "GET", "3", accountant)).Result);
        AssertDenied((await RunAsync("Seller", "Settlement", "Get", "GET", "3", accountant)).Result);
        var (_, http) = await RunAsync("Companion", "ClinicReserve", "Get", "GET", "3", accountant);
        Assert.Null(ProviderActingContext.Get(http));
    }

    [Fact]
    public async Task Staff_with_every_permission_still_cannot_define_roles_permissions_or_staff()
    {
        var all = Member(ProviderPermissionCatalog.Keys.ToArray());
        AssertDenied((await RunAsync("Companion", "ProviderUserRole", "Post", "POST", "3", all)).Result);
        AssertDenied((await RunAsync("Companion", "ProviderUserRole", "Put", "PUT", "3", all)).Result);
        AssertDenied((await RunAsync("Companion", "ProviderUser", "Post", "POST", "3", all)).Result);
        AssertDenied((await RunAsync("Companion", "ProviderPermission", "Get", "GET", "3", all)).Result);
        AssertDenied((await RunAsync("Companion", "Clinic", "Post", "POST", "3", all)).Result);
    }

    [Fact]
    public async Task View_permission_does_not_allow_writes()
    {
        AssertDenied((await RunAsync("Companion", "ClinicReserve", "Approve", "PUT", "3", Member("Clinic.Reserve.View"))).Result);
        Assert.Null((await RunAsync("Companion", "ClinicReserve", "Approve", "PUT", "3", Member("Clinic.Reserve.Manage"))).Result);
    }

    [Theory]
    [InlineData("4")]       // ارائه‌دهنده‌ی دیگر
    [InlineData("abc")]
    [InlineData("-3")]
    [InlineData("0")]
    public async Task Header_for_a_provider_the_user_does_not_work_for_is_denied(string header)
    {
        AssertDenied((await RunAsync("Companion", "ClinicReserve", "Get", "GET", header, Member("Clinic.Reserve.Manage"))).Result);
    }

    [Fact]
    public async Task Revoked_or_unaccepted_membership_is_denied()
    {
        AssertDenied((await RunAsync("Companion", "ClinicReserve", "Get", "GET", "3", membership: null)).Result);
    }

    [Fact]
    public async Task Personal_endpoints_ignore_the_header_and_run_as_the_real_user()
    {
        var (result, http) = await RunAsync("Companion", "ConsultationOnline", "Put", "PUT", "3", Member(), CompanionPanelEntry.StaffOnly);
        Assert.Null(result);
        Assert.Null(ProviderActingContext.Get(http));
    }

    [Fact]
    public async Task Venue_operators_without_header_reach_only_personal_or_operator_endpoints()
    {
        Assert.Null((await RunAsync("Companion", "ConsultationSession", "Get", "GET", null, null, CompanionPanelEntry.StaffOnly)).Result);
        Assert.Null((await RunAsync("Companion", "SchoolTeaching", "Attendance", "PUT", null, null, CompanionPanelEntry.StaffOnly)).Result);
        Assert.Null((await RunAsync("Companion", "CompanionReserveDebt", "MarkPaid", "PUT", null, null, CompanionPanelEntry.StaffOnly)).Result);
        AssertDenied((await RunAsync("Companion", "ClinicReserve", "Get", "GET", null, null, CompanionPanelEntry.StaffOnly)).Result);
        AssertDenied((await RunAsync("Companion", "ClinicSettlement", "Get", "GET", null, null, CompanionPanelEntry.StaffOnly)).Result);
        AssertDenied((await RunAsync("Companion", "ClinicUser", "Post", "POST", null, null, CompanionPanelEntry.StaffOnly)).Result);
    }

    [Fact]
    public async Task Provider_staff_without_header_get_nothing_and_owners_keep_their_panel()
    {
        AssertDenied((await RunAsync("Companion", "ClinicReserve", "Get", "GET", null, Member("Clinic.Reserve.Manage"))).Result);
        Assert.Null((await RunAsync("Companion", "ProviderUserRole", "Post", "POST", null, null, CompanionPanelEntry.Owner)).Result);
    }
}

/// <summary>سرویس ProviderUser روی LocalDB: فقط مالک نقش/کارمند می‌سازد، شناسه‌های ارائه‌دهنده‌ی دیگر پیدا نمی‌شوند، لغو دسترسی فوری است.</summary>
public class ProviderUserServiceTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public ProviderUserServiceTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private ProviderUserService NewService() =>
        new(_fixture.NewContext(), Microsoft.Extensions.Logging.Abstractions.NullLogger<ProviderUserService>.Instance);

    private long AddClinic(long ownerUserId, long providerId, long stateId, long cityId)
    {
        using var ctx = _fixture.NewContext();
        var clinic = new Entities.Entities.ClinicField.Clinic { Name = "C", OwnerId = ownerUserId, ProviderId = providerId, Phone = "1", AddressValue = "a", StateId = stateId, CityId = cityId, Active = true, Approved = true };
        ctx.Clinics.Add(clinic);
        ctx.SaveChanges();
        return clinic.Id;
    }

    [Fact]
    public async Task Owner_defines_roles_and_staff_only_get_what_the_role_grants_until_revoked()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed(); // owner = ارائه‌دهنده؛ other = کارمند آینده؛ liker = کاربر دیگر
        AddClinic(seed.OwnerUserId, seed.ProviderId, seed.StateId, seed.CityId);
        var service = NewService();

        // کاتالوگ فقط نمایندگی‌هایی را دارد که ارائه‌دهنده دارد
        var catalog = await service.CatalogAsync(seed.OwnerUserId);
        Assert.True(catalog.IsSuccess);
        Assert.Equal(new[] { ProviderRoleLabels.Clinic }, catalog.Data.Select(s => s.Key));

        // دسترسیِ نمایندگی‌ای که ارائه‌دهنده ندارد قبول نمی‌شود
        var bad = await service.SaveRoleAsync(seed.OwnerUserId, new ProviderUserRoleInputDto { Name = "x", PermissionKeys = new() { "Store.Order.View" } });
        Assert.False(bad.IsSuccess);

        var role = await service.SaveRoleAsync(seed.OwnerUserId, new ProviderUserRoleInputDto { Name = "حسابدار کلینیک", PermissionKeys = new() { "Clinic.Finance.View" } });
        Assert.True(role.IsSuccess);

        // کسی که مالک ارائه‌دهنده نیست (حتی کارمند) نمی‌تواند نقش بسازد یا دعوت کند
        Assert.False((await service.SaveRoleAsync(seed.OtherUserId, new ProviderUserRoleInputDto { Name = "y" })).IsSuccess);
        Assert.False((await service.InviteAsync(seed.OtherUserId, new ProviderUserInviteDto { Mobile = seed.OwnerMobile, ProviderUserRoleId = role.Data.Id })).IsSuccess);

        Assert.False((await service.InviteAsync(seed.OwnerUserId, new ProviderUserInviteDto { Mobile = seed.OwnerMobile, ProviderUserRoleId = role.Data.Id })).IsSuccess); // خودش
        var invite = await service.InviteAsync(seed.OwnerUserId, new ProviderUserInviteDto { Mobile = seed.OtherMobile, ProviderUserRoleId = role.Data.Id });
        Assert.True(invite.IsSuccess);
        Assert.False((await service.InviteAsync(seed.OwnerUserId, new ProviderUserInviteDto { Mobile = seed.OtherMobile, ProviderUserRoleId = role.Data.Id })).IsSuccess); // تکراری

        // تا نپذیرد دسترسی ندارد
        Assert.Null(await NewService().ResolveActingAsync(seed.OtherUserId, seed.ProviderId));
        Assert.Single(await service.ListInvitesAsync(seed.OtherUserId));
        Assert.False((await service.RespondInviteAsync(invite.Data.Id, seed.LikerUserId, true)).IsSuccess); // دعوتِ کس دیگر
        Assert.True((await service.RespondInviteAsync(invite.Data.Id, seed.OtherUserId, true)).IsSuccess);

        var access = await NewService().ResolveActingAsync(seed.OtherUserId, seed.ProviderId);
        Assert.NotNull(access);
        Assert.Equal(seed.OwnerUserId, access!.OwnerUserId);
        Assert.Equal(new[] { "Clinic.Finance.View" }, access.Permissions);
        var membership = Assert.Single(await NewService().ListMembershipsAsync(seed.OtherUserId));
        Assert.Equal(new[] { ProviderRoleLabels.Clinic }, membership.Scopes);

        // تغییر دسترسی نقش فوراً اثر می‌کند
        Assert.True((await service.SaveRoleAsync(seed.OwnerUserId, new ProviderUserRoleInputDto { Id = role.Data.Id, Name = "حسابدار کلینیک", PermissionKeys = new() { "Clinic.Reserve.View" } })).IsSuccess);
        Assert.Equal(new[] { "Clinic.Reserve.View" }, (await NewService().ResolveActingAsync(seed.OtherUserId, seed.ProviderId))!.Permissions);

        // نقشِ در حال استفاده حذف نمی‌شود
        Assert.False((await service.DeleteRoleAsync(seed.OwnerUserId, role.Data.Id)).IsSuccess);

        // غیرفعال‌کردن و حذف فوراً دسترسی را می‌بندد
        Assert.True((await service.SetActiveAsync(seed.OwnerUserId, invite.Data.Id, false)).IsSuccess);
        Assert.Null(await NewService().ResolveActingAsync(seed.OtherUserId, seed.ProviderId));
        Assert.True((await service.SetActiveAsync(seed.OwnerUserId, invite.Data.Id, true)).IsSuccess);
        Assert.NotNull(await NewService().ResolveActingAsync(seed.OtherUserId, seed.ProviderId));
        Assert.True((await service.RemoveAsync(seed.OwnerUserId, invite.Data.Id)).IsSuccess);
        Assert.Null(await NewService().ResolveActingAsync(seed.OtherUserId, seed.ProviderId));
        Assert.True((await service.DeleteRoleAsync(seed.OwnerUserId, role.Data.Id)).IsSuccess);
    }

    [Fact]
    public async Task Ids_of_another_provider_are_never_reachable()
    {
        if (!_fixture.Available) return;
        var a = _fixture.Seed();
        var b = _fixture.Seed();
        AddClinic(a.OwnerUserId, a.ProviderId, a.StateId, a.CityId);
        AddClinic(b.OwnerUserId, b.ProviderId, b.StateId, b.CityId);
        var service = NewService();

        var roleOfB = await service.SaveRoleAsync(b.OwnerUserId, new ProviderUserRoleInputDto { Name = "r", PermissionKeys = new() { "Clinic.Reserve.View" } });
        var staffOfB = await service.InviteAsync(b.OwnerUserId, new ProviderUserInviteDto { Mobile = b.OtherMobile, ProviderUserRoleId = roleOfB.Data.Id });
        Assert.True(staffOfB.IsSuccess);

        // مالک A با شناسه‌های B
        Assert.False((await service.InviteAsync(a.OwnerUserId, new ProviderUserInviteDto { Mobile = a.OtherMobile, ProviderUserRoleId = roleOfB.Data.Id })).IsSuccess);
        Assert.False((await service.SaveRoleAsync(a.OwnerUserId, new ProviderUserRoleInputDto { Id = roleOfB.Data.Id, Name = "hijack" })).IsSuccess);
        Assert.False((await service.DeleteRoleAsync(a.OwnerUserId, roleOfB.Data.Id)).IsSuccess);
        Assert.False((await service.SetActiveAsync(a.OwnerUserId, staffOfB.Data.Id, false)).IsSuccess);
        Assert.False((await service.ChangeRoleAsync(a.OwnerUserId, new ProviderUserRoleChangeDto { Id = staffOfB.Data.Id, ProviderUserRoleId = roleOfB.Data.Id })).IsSuccess);
        Assert.False((await service.RemoveAsync(a.OwnerUserId, staffOfB.Data.Id)).IsSuccess);
        Assert.DoesNotContain((await service.ListUsersAsync(a.OwnerUserId)).Data, u => u.Id == staffOfB.Data.Id);

        // کارمند B برای A هیچ دسترسی‌ای ندارد
        await service.RespondInviteAsync(staffOfB.Data.Id, b.OtherUserId, true);
        Assert.NotNull(await NewService().ResolveActingAsync(b.OtherUserId, b.ProviderId));
        Assert.Null(await NewService().ResolveActingAsync(b.OtherUserId, a.ProviderId));
    }
}
