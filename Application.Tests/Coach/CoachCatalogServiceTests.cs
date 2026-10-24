using Application.Common.Dto.LocationPoint;
using Application.Services.CoachSrvs.CoachCatalogSrv;
using Application.Services.CoachSrvs.CoachCatalogSrv.Dto;
using Application.Services.CoachSrvs.CoachSrv;
using Application.Services.CoachSrvs.CoachSrv.Dto;
using Entities.Entities;
using Entities.Entities.CoachField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Application.Tests.Coach;

public class CoachCatalogServiceTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public CoachCatalogServiceTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private CoachCatalogService NewService() =>
        new(_fixture.NewContext(), _fixture.Mapper, NullLogger<CoachCatalogService>.Instance);

    private async Task<(long CoachId, long AssistanceId, CoachLocalDbFixture.SeedData Seed)> SetupAsync()
    {
        var seed = _fixture.Seed();
        using var context = _fixture.NewContext();
        var assistance = new Assistance { Name = "Bath", Active = true, IsGrooming = false, ProviderRoleId = await AssistanceTestRoles.IdAsync(context, "Coach") };
        context.Assistances.Add(assistance);
        await context.SaveChangesAsync();
        var core = new CoachCoreService(context, _fixture.Mapper, NullLogger<CoachCoreService>.Instance);
        var coach = await core.CreateAsync(new CoachDto
        {
            Name = "Salon", ProviderId = seed.ProviderId, Phone = "021", AddressValue = "a",
            StateId = seed.StateId, CityId = seed.CityId, Location = new PointDto(51, 35),
        }, byAdmin: true, actorUserId: 1);
        Assert.True(coach.IsSuccess);
        return (coach.Data.Id, assistance.Id, seed);
    }

    [Fact]
    public async Task Service_OwnerAddsPending_AdminApproves_DuplicateRejected()
    {
        if (!_fixture.Available) return;
        var (coachId, assistanceId, seed) = await SetupAsync();
        var input = new CoachServiceInputDto { CoachId = coachId, AssistanceId = assistanceId, DurationMinutes = 45 };

        Assert.False((await NewService().AddServiceAsync(input, seed.OtherUserId)).IsSuccess);
        var added = await NewService().AddServiceAsync(input, seed.OwnerUserId);
        Assert.True(added.IsSuccess);
        Assert.False(added.Data.Approved);
        Assert.True(added.Data.Active);
        Assert.False((await NewService().AddServiceAsync(input, seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().AddServiceAsync(new CoachServiceInputDto { CoachId = coachId, AssistanceId = assistanceId + 999 }, null)).IsSuccess);

        var id = added.Data.Id;
        Assert.False((await NewService().ApproveServiceAsync(new CoachServiceApproveDto { Id = id, Approve = false })).IsSuccess);
        Assert.False((await NewService().ApproveServiceAsync(new CoachServiceApproveDto { Id = id, Approve = true, CommissionPercent = 120 })).IsSuccess);
        Assert.True((await NewService().ApproveServiceAsync(new CoachServiceApproveDto { Id = id, Approve = true, CommissionPercent = 15 })).IsSuccess);

        var listed = (await NewService().ListManageAsync(coachId, seed.OwnerUserId)).Data.Single();
        Assert.True(listed.Approved);
        Assert.Equal(15m, listed.CommissionPercent);
        Assert.False((await NewService().ListManageAsync(coachId, seed.OtherUserId)).IsSuccess);
    }

    [Fact]
    public async Task Package_ValidatesPriceAndSize_AndPublicListFiltersBySize()
    {
        if (!_fixture.Available) return;
        var (coachId, assistanceId, seed) = await SetupAsync();
        var service = (await NewService().AddServiceAsync(new CoachServiceInputDto { CoachId = coachId, AssistanceId = assistanceId }, null)).Data;

        CoachServicePackageInputDto Package(string name, double price, double pre, string size) => new()
            { CoachServiceId = service.Id, Name = name, Price = price, PrePaymentPrice = pre, PetSize = size };

        Assert.False((await NewService().AddPackageAsync(Package("zero", 0, 0, null), seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().AddPackageAsync(Package("pre>price", 100, 200, null), seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().AddPackageAsync(Package("bad size", 100, 0, "Huge"), seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().AddPackageAsync(Package(" ", 100, 0, null), seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().AddPackageAsync(Package("foreign", 100, 0, null), seed.OtherUserId)).IsSuccess);

        var small = await NewService().AddPackageAsync(Package("Small bath", 100000, 20000, "small"), seed.OwnerUserId);
        Assert.True(small.IsSuccess);
        Assert.Equal("Small", small.Data.PetSize);
        Assert.True((await NewService().AddPackageAsync(Package("Large bath", 300000, 0, "Large"), seed.OwnerUserId)).IsSuccess);
        Assert.True((await NewService().AddPackageAsync(Package("Any size", 50000, 0, null), seed.OwnerUserId)).IsSuccess);

        var forSmall = (await NewService().ListPublicAsync(coachId, "Small")).Data.Single();
        Assert.Equal(new[] { "Any size", "Small bath" }, forSmall.Packages.Select(p => p.Name).ToArray());
        var forLarge = (await NewService().ListPublicAsync(coachId, "Large")).Data.Single();
        Assert.Equal(new[] { "Any size", "Large bath" }, forLarge.Packages.Select(p => p.Name).ToArray());

        Assert.True((await NewService().SetPackageActiveAsync(small.Data.Id, false, seed.OwnerUserId)).IsSuccess);
        Assert.DoesNotContain("Small bath", (await NewService().ListPublicAsync(coachId, "Small")).Data.Single().Packages.Select(p => p.Name));
        Assert.True((await NewService().DeletePackageAsync(small.Data.Id, seed.OwnerUserId)).IsSuccess);
        Assert.DoesNotContain("Small bath", (await NewService().ListManageAsync(coachId, null)).Data.Single().Packages.Select(p => p.Name));
    }

    [Fact]
    public async Task PublicList_HidesUnapprovedInactiveDeletedServices_AndInactiveCoach()
    {
        if (!_fixture.Available) return;
        var (coachId, assistanceId, seed) = await SetupAsync();
        var service = (await NewService().AddServiceAsync(new CoachServiceInputDto { CoachId = coachId, AssistanceId = assistanceId }, seed.OwnerUserId)).Data;
        Assert.True((await NewService().AddPackageAsync(new CoachServicePackageInputDto { CoachServiceId = service.Id, Name = "p", Price = 10 }, seed.OwnerUserId)).IsSuccess);

        Assert.Empty((await NewService().ListPublicAsync(coachId, null)).Data); // هنوز تأیید نشده
        await NewService().ApproveServiceAsync(new CoachServiceApproveDto { Id = service.Id, Approve = true });
        Assert.Single((await NewService().ListPublicAsync(coachId, null)).Data);

        await NewService().SetServiceActiveAsync(service.Id, false, seed.OwnerUserId);
        Assert.Empty((await NewService().ListPublicAsync(coachId, null)).Data);
        await NewService().SetServiceActiveAsync(service.Id, true, seed.OwnerUserId);

        using (var context = _fixture.NewContext())
        {
            var core = new CoachCoreService(context, _fixture.Mapper, NullLogger<CoachCoreService>.Instance);
            await core.UpdateActiveAsync(new CoachActiveDto { Id = coachId, Active = false }, null);
        }
        Assert.Empty((await NewService().ListPublicAsync(coachId, null)).Data);
    }

    [Fact]
    public async Task ServiceUsers_OnlyOwnerOrActiveStaff()
    {
        if (!_fixture.Available) return;
        var (coachId, assistanceId, seed) = await SetupAsync();
        var service = (await NewService().AddServiceAsync(new CoachServiceInputDto { CoachId = coachId, AssistanceId = assistanceId }, null)).Data;

        Assert.False((await NewService().SetServiceUsersAsync(service.Id, new[] { seed.OtherUserId }, seed.OwnerUserId)).IsSuccess);
        using (var context = _fixture.NewContext())
        {
            context.CoachUsers.Add(new CoachUser { CoachId = coachId, UserId = seed.OtherUserId, Active = true, UserAccept = true });
            await context.SaveChangesAsync();
        }
        Assert.True((await NewService().SetServiceUsersAsync(service.Id, new[] { seed.OtherUserId, seed.OwnerUserId, seed.OtherUserId }, seed.OwnerUserId)).IsSuccess);
        Assert.Equal(2, (await NewService().ListManageAsync(coachId, null)).Data.Single().UserIds.Count);
        Assert.True((await NewService().SetServiceUsersAsync(service.Id, Array.Empty<long>(), null)).IsSuccess);
        Assert.Empty((await NewService().ListManageAsync(coachId, null)).Data.Single().UserIds);
        Assert.False((await NewService().SetServiceUsersAsync(service.Id, new[] { seed.OwnerUserId }, seed.LikerUserId)).IsSuccess);
    }
}
