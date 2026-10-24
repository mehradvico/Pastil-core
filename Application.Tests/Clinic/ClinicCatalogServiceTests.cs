using Application.Common.Dto.LocationPoint;
using Application.Services.ClinicSrvs.ClinicCatalogSrv;
using Application.Services.ClinicSrvs.ClinicCatalogSrv.Dto;
using Application.Services.ClinicSrvs.ClinicSrv;
using Application.Services.ClinicSrvs.ClinicSrv.Dto;
using Entities.Entities;
using Entities.Entities.ClinicField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Application.Tests.Clinic;

public class ClinicCatalogServiceTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;

    public ClinicCatalogServiceTests(ClinicLocalDbFixture fixture) => _fixture = fixture;

    private ClinicCatalogService NewService() =>
        new(_fixture.NewContext(), _fixture.Mapper, NullLogger<ClinicCatalogService>.Instance);

    private async Task<(long ClinicId, long AssistanceId, ClinicLocalDbFixture.SeedData Seed)> SetupAsync()
    {
        var seed = _fixture.Seed();
        using var context = _fixture.NewContext();
        var assistance = new Assistance { Name = "Bath", Active = true, IsGrooming = false, ProviderRoleId = await AssistanceTestRoles.IdAsync(context, "Clinic") };
        context.Assistances.Add(assistance);
        await context.SaveChangesAsync();
        var core = new ClinicCoreService(context, _fixture.Mapper, NullLogger<ClinicCoreService>.Instance);
        var clinic = await core.CreateAsync(new ClinicDto
        {
            Name = "Salon", ProviderId = seed.ProviderId, Phone = "021", AddressValue = "a",
            StateId = seed.StateId, CityId = seed.CityId, Location = new PointDto(51, 35),
        }, byAdmin: true, actorUserId: 1);
        Assert.True(clinic.IsSuccess);
        return (clinic.Data.Id, assistance.Id, seed);
    }

    [Fact]
    public async Task Service_OwnerAddsPending_AdminApproves_DuplicateRejected()
    {
        if (!_fixture.Available) return;
        var (clinicId, assistanceId, seed) = await SetupAsync();
        var input = new ClinicServiceInputDto { ClinicId = clinicId, AssistanceId = assistanceId, DurationMinutes = 45 };

        Assert.False((await NewService().AddServiceAsync(input, seed.OtherUserId)).IsSuccess);
        var added = await NewService().AddServiceAsync(input, seed.OwnerUserId);
        Assert.True(added.IsSuccess);
        Assert.False(added.Data.Approved);
        Assert.True(added.Data.Active);
        Assert.False((await NewService().AddServiceAsync(input, seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().AddServiceAsync(new ClinicServiceInputDto { ClinicId = clinicId, AssistanceId = assistanceId + 999 }, null)).IsSuccess);

        var id = added.Data.Id;
        Assert.False((await NewService().ApproveServiceAsync(new ClinicServiceApproveDto { Id = id, Approve = false })).IsSuccess);
        Assert.False((await NewService().ApproveServiceAsync(new ClinicServiceApproveDto { Id = id, Approve = true, CommissionPercent = 120 })).IsSuccess);
        Assert.True((await NewService().ApproveServiceAsync(new ClinicServiceApproveDto { Id = id, Approve = true, CommissionPercent = 15 })).IsSuccess);

        var listed = (await NewService().ListManageAsync(clinicId, seed.OwnerUserId)).Data.Single();
        Assert.True(listed.Approved);
        Assert.Equal(15m, listed.CommissionPercent);
        Assert.False((await NewService().ListManageAsync(clinicId, seed.OtherUserId)).IsSuccess);
    }

    [Fact]
    public async Task Package_ValidatesPriceAndSize_AndPublicListFiltersBySize()
    {
        if (!_fixture.Available) return;
        var (clinicId, assistanceId, seed) = await SetupAsync();
        var service = (await NewService().AddServiceAsync(new ClinicServiceInputDto { ClinicId = clinicId, AssistanceId = assistanceId }, null)).Data;

        ClinicServicePackageInputDto Package(string name, double price, double pre, string size) => new()
            { ClinicServiceId = service.Id, Name = name, Price = price, PrePaymentPrice = pre, PetSize = size };

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

        var forSmall = (await NewService().ListPublicAsync(clinicId, "Small")).Data.Single();
        Assert.Equal(new[] { "Any size", "Small bath" }, forSmall.Packages.Select(p => p.Name).ToArray());
        var forLarge = (await NewService().ListPublicAsync(clinicId, "Large")).Data.Single();
        Assert.Equal(new[] { "Any size", "Large bath" }, forLarge.Packages.Select(p => p.Name).ToArray());

        Assert.True((await NewService().SetPackageActiveAsync(small.Data.Id, false, seed.OwnerUserId)).IsSuccess);
        Assert.DoesNotContain("Small bath", (await NewService().ListPublicAsync(clinicId, "Small")).Data.Single().Packages.Select(p => p.Name));
        Assert.True((await NewService().DeletePackageAsync(small.Data.Id, seed.OwnerUserId)).IsSuccess);
        Assert.DoesNotContain("Small bath", (await NewService().ListManageAsync(clinicId, null)).Data.Single().Packages.Select(p => p.Name));
    }

    [Fact]
    public async Task PublicList_HidesUnapprovedInactiveDeletedServices_AndInactiveClinic()
    {
        if (!_fixture.Available) return;
        var (clinicId, assistanceId, seed) = await SetupAsync();
        var service = (await NewService().AddServiceAsync(new ClinicServiceInputDto { ClinicId = clinicId, AssistanceId = assistanceId }, seed.OwnerUserId)).Data;
        Assert.True((await NewService().AddPackageAsync(new ClinicServicePackageInputDto { ClinicServiceId = service.Id, Name = "p", Price = 10 }, seed.OwnerUserId)).IsSuccess);

        Assert.Empty((await NewService().ListPublicAsync(clinicId, null)).Data); // هنوز تأیید نشده
        await NewService().ApproveServiceAsync(new ClinicServiceApproveDto { Id = service.Id, Approve = true });
        Assert.Single((await NewService().ListPublicAsync(clinicId, null)).Data);

        await NewService().SetServiceActiveAsync(service.Id, false, seed.OwnerUserId);
        Assert.Empty((await NewService().ListPublicAsync(clinicId, null)).Data);
        await NewService().SetServiceActiveAsync(service.Id, true, seed.OwnerUserId);

        using (var context = _fixture.NewContext())
        {
            var core = new ClinicCoreService(context, _fixture.Mapper, NullLogger<ClinicCoreService>.Instance);
            await core.UpdateActiveAsync(new ClinicActiveDto { Id = clinicId, Active = false }, null);
        }
        Assert.Empty((await NewService().ListPublicAsync(clinicId, null)).Data);
    }

    [Fact]
    public async Task ServiceUsers_OnlyOwnerOrActiveStaff()
    {
        if (!_fixture.Available) return;
        var (clinicId, assistanceId, seed) = await SetupAsync();
        var service = (await NewService().AddServiceAsync(new ClinicServiceInputDto { ClinicId = clinicId, AssistanceId = assistanceId }, null)).Data;

        Assert.False((await NewService().SetServiceUsersAsync(service.Id, new[] { seed.OtherUserId }, seed.OwnerUserId)).IsSuccess);
        using (var context = _fixture.NewContext())
        {
            context.ClinicUsers.Add(new ClinicUser { ClinicId = clinicId, UserId = seed.OtherUserId, Active = true, UserAccept = true });
            await context.SaveChangesAsync();
        }
        Assert.True((await NewService().SetServiceUsersAsync(service.Id, new[] { seed.OtherUserId, seed.OwnerUserId, seed.OtherUserId }, seed.OwnerUserId)).IsSuccess);
        Assert.Equal(2, (await NewService().ListManageAsync(clinicId, null)).Data.Single().UserIds.Count);
        Assert.True((await NewService().SetServiceUsersAsync(service.Id, Array.Empty<long>(), null)).IsSuccess);
        Assert.Empty((await NewService().ListManageAsync(clinicId, null)).Data.Single().UserIds);
        Assert.False((await NewService().SetServiceUsersAsync(service.Id, new[] { seed.OwnerUserId }, seed.LikerUserId)).IsSuccess);
    }
}
