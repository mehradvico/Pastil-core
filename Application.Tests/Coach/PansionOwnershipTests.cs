using Application.Common.Dto.LocationPoint;
using Application.Services.FinanceSrvs.SettlementSrv;
using Application.Services.PansionSrvs.PansionOwnershipSrv;
using Application.Services.PansionSrvs.PansionSrv;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Entities.Entities;
using Entities.Entities.PansionField;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.Tests.Coach;

/// <summary>
/// مالک پانسیون ارائه‌دهنده است (Pansion.ProviderId، ۲۰۲۶-۱۱-۰۳)؛ پانسیون قدیمیِ زیر نمایندگی هم مال مالک همان نمایندگی می‌ماند.
/// </summary>
public class PansionOwnershipTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public PansionOwnershipTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private PansionOwnershipService NewOwnership() => new(_fixture.NewContext());

    private PansionService NewPansionService() => new(
        _fixture.NewContext(), _fixture.Mapper,
        FakeProxy<Application.Services.Setting.NoticeSrv.Iface.INoticeService>.Create().Instance,
        FakeProxy<Application.Services.CommonSrv.PushNotificationSrv.Iface.IPushNotificationService>.Create().Instance,
        Microsoft.Extensions.Logging.Abstractions.NullLogger<PansionService>.Instance);

    private async Task<long> AddPansionAsync(CoachLocalDbFixture.SeedData seed, long? companionId, long? providerId)
    {
        using var context = _fixture.NewContext();
        var pansion = new Pansion { Name = "Pansion " + Guid.NewGuid().ToString("N")[..4], CompanionId = companionId, ProviderId = providerId, Active = true, StateId = seed.StateId, CityId = seed.CityId, AddressValue = "a" };
        context.Pansions.Add(pansion);
        await context.SaveChangesAsync();
        return pansion.Id;
    }

    private async Task<long> AddCompanionAsync(CoachLocalDbFixture.SeedData seed, bool approved, long? providerId)
    {
        using var context = _fixture.NewContext();
        var companion = new Companion { Name = "Pan", ReferralCode = "P" + Guid.NewGuid().ToString("N")[..8], OwnerId = seed.OwnerUserId, CityId = seed.CityId, ProviderId = providerId, Phone = "021", AddressValue = "a", Active = approved, Approved = approved };
        context.Companions.Add(companion);
        await context.SaveChangesAsync();
        return companion.Id;
    }

    private async Task GrantPansionRoleAsync(long providerId)
    {
        using var context = _fixture.NewContext();
        var role = await context.ProviderRoles.FirstOrDefaultAsync(r => r.Label == Entities.Entities.ProviderField.ProviderRoleLabels.Pansion);
        if (role == null)
        {
            role = new Entities.Entities.ProviderField.ProviderRole { Name = "Pansion", Label = Entities.Entities.ProviderField.ProviderRoleLabels.Pansion, Active = true };
            context.ProviderRoles.Add(role);
            await context.SaveChangesAsync();
        }
        context.ProviderRoleAssignments.Add(new Entities.Entities.ProviderField.ProviderRoleAssignment { ProviderId = providerId, ProviderRoleId = role.Id, Approved = true });
        await context.SaveChangesAsync();
    }

    private static PansionDto NewRequest(CoachLocalDbFixture.SeedData seed, long? providerId, long? companionId) => new()
    {
        Name = "New pansion",
        IsDaycare = false,
        StateId = seed.StateId,
        CityId = seed.CityId,
        AddressValue = "addr",
        Location = new PointDto(51.4, 35.7),
        OpenHour = "08:00",
        CloseHour = "20:00",
        PansionPrice = 100000,
        ProviderId = providerId,
        CompanionId = companionId,
    };

    [Fact]
    public async Task LegacyCompanionPansion_FollowsCompanionOwner_AndApprovalGatesWrites()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var companionId = await AddCompanionAsync(seed, approved: false, providerId: null);
        var pansionId = await AddPansionAsync(seed, companionId, providerId: null);
        var ownership = NewOwnership();

        // نمایندگی تأییدنشده: خواندن آزاد، نوشتن بسته
        Assert.Null(await ownership.PansionOfPansionAsync(pansionId, seed.OwnerUserId));
        Assert.Equal(pansionId, await ownership.PansionOfPansionAsync(pansionId, seed.OwnerUserId, approvedOnly: false));

        using (var ctx = _fixture.NewContext())
            await ctx.Companions.Where(c => c.Id == companionId).ExecuteUpdateAsync(s => s.SetProperty(c => c.Active, true).SetProperty(c => c.Approved, true));
        Assert.Equal(pansionId, await ownership.PansionOfPansionAsync(pansionId, seed.OwnerUserId));

        // غریبه هیچ‌چیز ندارد
        Assert.Null(await ownership.PansionOfPansionAsync(pansionId, seed.OtherUserId, approvedOnly: false));
        Assert.Null(await ownership.PansionOfReserveAsync(123456, seed.OwnerUserId));

        // بدون نقش «پانسیون»: پانسیون جدید زیر همان نمایندگی (مسیر قدیمی)
        var legacy = await ownership.ResolveOwnerForNewPansionAsync(seed.OwnerUserId);
        Assert.True(legacy.Ok);
        Assert.Null(legacy.ProviderId);
        Assert.Equal(companionId, legacy.CompanionId);
    }

    [Fact]
    public async Task ProviderWithPansionRole_OwnsPansions_WithoutAnyCompanion()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var ownership = NewOwnership();

        // بدون نقش و بدون نمایندگی: پیام روشن
        Assert.Equal(Resource.Notification.PansionNeedsProviderRole, (await ownership.ResolveOwnerForNewPansionAsync(seed.OwnerUserId)).Error);

        // ادمین: ارائه‌دهنده بدون نقش «پانسیون» پذیرفته نمی‌شود
        var rejected = await NewPansionService().InsertAsyncDto(NewRequest(seed, seed.ProviderId, null));
        Assert.False(rejected.IsSuccess);
        Assert.Contains(Resource.Notification.ProviderRoleRequired, rejected.Messages.Select(m => m.Item1));

        await GrantPansionRoleAsync(seed.ProviderId);
        var owner = await ownership.ResolveOwnerForNewPansionAsync(seed.OwnerUserId);
        Assert.True(owner.Ok);
        Assert.Equal(seed.ProviderId, owner.ProviderId);
        Assert.Null(owner.CompanionId);

        var created = await NewPansionService().InsertAsyncDto(NewRequest(seed, seed.ProviderId, null));
        Assert.True(created.IsSuccess);
        var pansionId = created.Data.Id;
        using (var ctx = _fixture.NewContext())
        {
            var row = await ctx.Pansions.AsNoTracking().SingleAsync(p => p.Id == pansionId);
            Assert.Equal(seed.ProviderId, row.ProviderId);
            Assert.Null(row.CompanionId);
            Assert.False(row.Active); // درخواست تا تأیید ادمین غیرفعال است
        }

        Assert.Equal(pansionId, await ownership.PansionOfPansionAsync(pansionId, seed.OwnerUserId));
        Assert.Null(await ownership.PansionOfPansionAsync(pansionId, seed.OtherUserId, approvedOnly: false));
        Assert.Contains(pansionId, NewPansionService().Search(new PansionInputDto { OwnerUserId = seed.OwnerUserId, PageIndex = 1, PageSize = 50 }).List.Select(s => s.Id));
        Assert.DoesNotContain(pansionId, NewPansionService().Search(new PansionInputDto { OwnerUserId = seed.OtherUserId, PageIndex = 1, PageSize = 50 }).List.Select(s => s.Id));

        // ارائه‌دهنده‌ی غیرفعال: نوشتن بسته، خواندن مال خودش آزاد
        using (var ctx = _fixture.NewContext())
            await ctx.Providers.Where(p => p.Id == seed.ProviderId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Active, false));
        Assert.Null(await ownership.PansionOfPansionAsync(pansionId, seed.OwnerUserId));
        Assert.Equal(pansionId, await ownership.PansionOfPansionAsync(pansionId, seed.OwnerUserId, approvedOnly: false));
    }

    [Fact]
    public async Task AdminCreate_WithoutAnyOwner_IsRejected()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var result = await NewPansionService().InsertAsyncDto(NewRequest(seed, null, null));
        Assert.False(result.IsSuccess);
        Assert.Contains(Resource.Notification.PansionProviderRequired, result.Messages.Select(m => m.Item1));
    }

    [Fact]
    public void SettlementPools_SplitByProvider()
    {
        // استخر نمایندگی فقط پانسیون بدون ارائه‌دهنده را می‌شمارد؛ استخر «پانسیون» فقط پانسیون دارای ارائه‌دهنده را
        var options = new DbContextOptionsBuilder<Persistence.Context.DataBaseContext>()
            .UseSqlServer("Server=(local);Database=TranslationOnly;Integrated Security=true;TrustServerCertificate=true", sql => sql.UseNetTopologySuite())
            .Options;
        using var context = new Persistence.Context.DataBaseContext(options);

        var legacy = SettlementSources.PansionReserves(context, new long[] { 1 }).ToQueryString();
        var provider = SettlementSources.ProviderPansionReserves(context, 7).ToQueryString();

        Assert.Contains("[ProviderId] IS NULL", legacy);
        Assert.Contains("[ProviderId] =", provider);
        Assert.Contains("StatusId", provider);
        Assert.Contains("OwnerDecision", provider);
    }
}
