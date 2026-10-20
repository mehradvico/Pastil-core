using Application.Services.ProviderSrvs.ProviderSubscriptionSyncSrv.Iface;
using Application.Services.ProviderSrvs.ProviderVenueSrv;
using Application.Services.ProviderSrvs.ProviderVenueSrv.Dto;
using Entities.Entities.CoachField;
using Entities.Entities.ProviderField;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Application.Tests.Coach.CoachReserveTestKit;

namespace Application.Tests.Coach;

/// <summary>هر مرکز فقط وقتی به ارائه‌دهنده وصل می‌شود که نقش تأییدشده‌ی همان کارکرد را داشته باشد.</summary>
public class ProviderVenueRoleTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public ProviderVenueRoleTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private ProviderVenueService NewService() =>
        new(_fixture.NewContext(), FakeProxy<IProviderSubscriptionSyncService>.Create().Instance);

    private async Task<long> AddCoachAsync(long providerId)
    {
        using var context = _fixture.NewContext();
        var seed = _fixture.Seed();
        var coach = new Entities.Entities.CoachField.Coach
        {
            Name = "Venue", ProviderId = null, OwnerId = seed.OwnerUserId, Phone = "021", AddressValue = "a",
            StateId = seed.StateId, CityId = seed.CityId, Active = true, Approved = true,
        };
        context.Coaches.Add(coach);
        await context.SaveChangesAsync();
        return coach.Id;
    }

    [Fact]
    public async Task AssignCoach_RequiresApprovedCoachRole()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var coachId = await AddCoachAsync(seed.ProviderId);

        var denied = await NewService().AssignCoachAsync(new ProviderVenueAssignDto { ProviderId = seed.ProviderId, Id = coachId });
        Assert.False(denied.IsSuccess);

        long roleId;
        using (var context = _fixture.NewContext())
        {
            var role = new ProviderRole { Name = "Coach", Label = ProviderRoleLabels.Coach + "_" + Guid.NewGuid().ToString("N")[..6], Active = true };
            context.ProviderRoles.Add(role);
            await context.SaveChangesAsync();
            roleId = role.Id;
            context.ProviderRoleAssignments.Add(new ProviderRoleAssignment { ProviderId = seed.ProviderId, ProviderRoleId = roleId, Approved = true });
            await context.SaveChangesAsync();
        }
        // برچسب نقش هنوز Coach نیست (پسوند تصادفی دارد) پس همچنان رد می‌شود.
        Assert.False((await NewService().AssignCoachAsync(new ProviderVenueAssignDto { ProviderId = seed.ProviderId, Id = coachId })).IsSuccess);

        using (var context = _fixture.NewContext())
            await context.ProviderRoles.Where(r => r.Id == roleId).ExecuteUpdateAsync(s => s.SetProperty(r => r.Label, ProviderRoleLabels.Coach));
        var ok = await NewService().AssignCoachAsync(new ProviderVenueAssignDto { ProviderId = seed.ProviderId, Id = coachId });
        Assert.True(ok.IsSuccess);

        // نقش تأییدنشده کافی نیست
        using (var context = _fixture.NewContext())
            await context.ProviderRoleAssignments.Where(a => a.ProviderRoleId == roleId).ExecuteUpdateAsync(s => s.SetProperty(a => a.Approved, false));
        var coach2 = await AddCoachAsync(seed.ProviderId);
        Assert.False((await NewService().AssignCoachAsync(new ProviderVenueAssignDto { ProviderId = seed.ProviderId, Id = coach2 })).IsSuccess);
    }
}
