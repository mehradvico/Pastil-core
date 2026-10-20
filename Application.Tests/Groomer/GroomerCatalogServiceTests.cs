using Application.Common.Dto.LocationPoint;
using Application.Services.GroomerSrvs.GroomerCatalogSrv;
using Application.Services.GroomerSrvs.GroomerCatalogSrv.Dto;
using Application.Services.GroomerSrvs.GroomerSrv;
using Application.Services.GroomerSrvs.GroomerSrv.Dto;
using Entities.Entities;
using Entities.Entities.GroomerField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Application.Tests.Groomer;

public class GroomerCatalogServiceTests : IClassFixture<GroomerLocalDbFixture>
{
    private readonly GroomerLocalDbFixture _fixture;

    public GroomerCatalogServiceTests(GroomerLocalDbFixture fixture) => _fixture = fixture;

    private GroomerCatalogService NewService() =>
        new(_fixture.NewContext(), _fixture.Mapper, NullLogger<GroomerCatalogService>.Instance);

    private async Task<(long GroomerId, long AssistanceId, GroomerLocalDbFixture.SeedData Seed)> SetupAsync()
    {
        var seed = _fixture.Seed();
        using var context = _fixture.NewContext();
        var assistance = new Assistance { Name = "Bath", Active = true, IsGrooming = true };
        context.Assistances.Add(assistance);
        await context.SaveChangesAsync();
        var core = new GroomerCoreService(context, _fixture.Mapper, NullLogger<GroomerCoreService>.Instance);
        var groomer = await core.CreateAsync(new GroomerDto
        {
            Name = "Salon", ProviderId = seed.ProviderId, Phone = "021", AddressValue = "a",
            StateId = seed.StateId, CityId = seed.CityId, Location = new PointDto(51, 35),
        }, byAdmin: true, actorUserId: 1);
        Assert.True(groomer.IsSuccess);
        return (groomer.Data.Id, assistance.Id, seed);
    }

    [Fact]
    public async Task Service_OwnerAddsPending_AdminApproves_DuplicateRejected()
    {
        if (!_fixture.Available) return;
        var (groomerId, assistanceId, seed) = await SetupAsync();
        var input = new GroomerServiceInputDto { GroomerId = groomerId, AssistanceId = assistanceId, DurationMinutes = 45 };

        Assert.False((await NewService().AddServiceAsync(input, seed.OtherUserId)).IsSuccess);
        var added = await NewService().AddServiceAsync(input, seed.OwnerUserId);
        Assert.True(added.IsSuccess);
        Assert.False(added.Data.Approved);
        Assert.True(added.Data.Active);
        Assert.False((await NewService().AddServiceAsync(input, seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().AddServiceAsync(new GroomerServiceInputDto { GroomerId = groomerId, AssistanceId = assistanceId + 999 }, null)).IsSuccess);

        var id = added.Data.Id;
        Assert.False((await NewService().ApproveServiceAsync(new GroomerServiceApproveDto { Id = id, Approve = false })).IsSuccess);
        Assert.False((await NewService().ApproveServiceAsync(new GroomerServiceApproveDto { Id = id, Approve = true, CommissionPercent = 120 })).IsSuccess);
        Assert.True((await NewService().ApproveServiceAsync(new GroomerServiceApproveDto { Id = id, Approve = true, CommissionPercent = 15 })).IsSuccess);

        var listed = (await NewService().ListManageAsync(groomerId, seed.OwnerUserId)).Data.Single();
        Assert.True(listed.Approved);
        Assert.Equal(15m, listed.CommissionPercent);
        Assert.False((await NewService().ListManageAsync(groomerId, seed.OtherUserId)).IsSuccess);
    }

    [Fact]
    public async Task Package_ValidatesPriceAndSize_AndPublicListFiltersBySize()
    {
        if (!_fixture.Available) return;
        var (groomerId, assistanceId, seed) = await SetupAsync();
        var service = (await NewService().AddServiceAsync(new GroomerServiceInputDto { GroomerId = groomerId, AssistanceId = assistanceId }, null)).Data;

        GroomerServicePackageInputDto Package(string name, double price, double pre, string size) => new()
            { GroomerServiceId = service.Id, Name = name, Price = price, PrePaymentPrice = pre, PetSize = size };

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

        var forSmall = (await NewService().ListPublicAsync(groomerId, "Small")).Data.Single();
        Assert.Equal(new[] { "Any size", "Small bath" }, forSmall.Packages.Select(p => p.Name).ToArray());
        var forLarge = (await NewService().ListPublicAsync(groomerId, "Large")).Data.Single();
        Assert.Equal(new[] { "Any size", "Large bath" }, forLarge.Packages.Select(p => p.Name).ToArray());

        Assert.True((await NewService().SetPackageActiveAsync(small.Data.Id, false, seed.OwnerUserId)).IsSuccess);
        Assert.DoesNotContain("Small bath", (await NewService().ListPublicAsync(groomerId, "Small")).Data.Single().Packages.Select(p => p.Name));
        Assert.True((await NewService().DeletePackageAsync(small.Data.Id, seed.OwnerUserId)).IsSuccess);
        Assert.DoesNotContain("Small bath", (await NewService().ListManageAsync(groomerId, null)).Data.Single().Packages.Select(p => p.Name));
    }

    [Fact]
    public async Task PublicList_HidesUnapprovedInactiveDeletedServices_AndInactiveGroomer()
    {
        if (!_fixture.Available) return;
        var (groomerId, assistanceId, seed) = await SetupAsync();
        var service = (await NewService().AddServiceAsync(new GroomerServiceInputDto { GroomerId = groomerId, AssistanceId = assistanceId }, seed.OwnerUserId)).Data;
        Assert.True((await NewService().AddPackageAsync(new GroomerServicePackageInputDto { GroomerServiceId = service.Id, Name = "p", Price = 10 }, seed.OwnerUserId)).IsSuccess);

        Assert.Empty((await NewService().ListPublicAsync(groomerId, null)).Data); // هنوز تأیید نشده
        await NewService().ApproveServiceAsync(new GroomerServiceApproveDto { Id = service.Id, Approve = true });
        Assert.Single((await NewService().ListPublicAsync(groomerId, null)).Data);

        await NewService().SetServiceActiveAsync(service.Id, false, seed.OwnerUserId);
        Assert.Empty((await NewService().ListPublicAsync(groomerId, null)).Data);
        await NewService().SetServiceActiveAsync(service.Id, true, seed.OwnerUserId);

        using (var context = _fixture.NewContext())
        {
            var core = new GroomerCoreService(context, _fixture.Mapper, NullLogger<GroomerCoreService>.Instance);
            await core.UpdateActiveAsync(new GroomerActiveDto { Id = groomerId, Active = false }, null);
        }
        Assert.Empty((await NewService().ListPublicAsync(groomerId, null)).Data);
    }

    [Fact]
    public async Task ServiceUsers_OnlyOwnerOrActiveStaff()
    {
        if (!_fixture.Available) return;
        var (groomerId, assistanceId, seed) = await SetupAsync();
        var service = (await NewService().AddServiceAsync(new GroomerServiceInputDto { GroomerId = groomerId, AssistanceId = assistanceId }, null)).Data;

        Assert.False((await NewService().SetServiceUsersAsync(service.Id, new[] { seed.OtherUserId }, seed.OwnerUserId)).IsSuccess);
        using (var context = _fixture.NewContext())
        {
            context.GroomerUsers.Add(new GroomerUser { GroomerId = groomerId, UserId = seed.OtherUserId, Active = true, UserAccept = true });
            await context.SaveChangesAsync();
        }
        Assert.True((await NewService().SetServiceUsersAsync(service.Id, new[] { seed.OtherUserId, seed.OwnerUserId, seed.OtherUserId }, seed.OwnerUserId)).IsSuccess);
        Assert.Equal(2, (await NewService().ListManageAsync(groomerId, null)).Data.Single().UserIds.Count);
        Assert.True((await NewService().SetServiceUsersAsync(service.Id, Array.Empty<long>(), null)).IsSuccess);
        Assert.Empty((await NewService().ListManageAsync(groomerId, null)).Data.Single().UserIds);
        Assert.False((await NewService().SetServiceUsersAsync(service.Id, new[] { seed.OwnerUserId }, seed.LikerUserId)).IsSuccess);
    }
}
