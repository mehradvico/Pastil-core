using Application.Common.Dto.LocationPoint;
using Application.Services.ClinicSrvs.ClinicSrv;
using Application.Services.ClinicSrvs.ClinicSrv.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Application.Tests.Clinic;

public class ClinicCoreServiceTests : IClassFixture<ClinicLocalDbFixture>
{
    private readonly ClinicLocalDbFixture _fixture;

    public ClinicCoreServiceTests(ClinicLocalDbFixture fixture) => _fixture = fixture;

    private ClinicCoreService NewService(Persistence.Context.DataBaseContext context) =>
        new(context, _fixture.Mapper, NullLogger<ClinicCoreService>.Instance);

    private static ClinicDto ValidDto(ClinicLocalDbFixture.SeedData seed, string name = "Salon") => new()
    {
        Name = name,
        ProviderId = seed.ProviderId,
        Phone = "02100000000",
        AddressValue = "Somewhere 1",
        StateId = seed.StateId,
        CityId = seed.CityId,
        Location = new PointDto(51.4, 35.7),
        ServiceInPerson = true,
        PictureId = seed.PictureIds[0],
    };

    [Fact]
    public async Task Create_ByAdmin_IsApprovedActiveAndOwnedByProviderUser()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        using var context = _fixture.NewContext();
        var result = await NewService(context).CreateAsync(ValidDto(seed), byAdmin: true, actorUserId: 999);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data.Approved);
        Assert.True(result.Data.Active);
        Assert.False(result.Data.ShowToSite);
        Assert.Equal(seed.OwnerUserId, result.Data.OwnerId);
        Assert.Equal(0, result.Data.RateCount);
    }

    [Fact]
    public async Task Create_ByProvider_WaitsForApproval()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        using var context = _fixture.NewContext();
        var result = await NewService(context).CreateAsync(ValidDto(seed), byAdmin: false, actorUserId: seed.OwnerUserId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Data.Approved);
        Assert.False(result.Data.Active);
    }

    [Fact]
    public async Task Create_ByAnotherUserWithForeignProvider_IsDenied()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        using var context = _fixture.NewContext();
        var result = await NewService(context).CreateAsync(ValidDto(seed), byAdmin: false, actorUserId: seed.OtherUserId);

        Assert.False(result.IsSuccess);
        Assert.Equal(0, await context.Clinics.CountAsync(g => g.ProviderId == seed.ProviderId));
    }

    [Fact]
    public async Task Create_WithoutRequiredFieldsOrWrongCity_Fails()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        using var context = _fixture.NewContext();
        var service = NewService(context);

        var noName = ValidDto(seed, "  ");
        Assert.False((await service.CreateAsync(noName, true, 1)).IsSuccess);

        var wrongCity = ValidDto(seed);
        wrongCity.CityId = seed.CityId + 1000;
        Assert.False((await service.CreateAsync(wrongCity, true, 1)).IsSuccess);

        var noLocation = ValidDto(seed);
        noLocation.Location = null;
        Assert.False((await service.CreateAsync(noLocation, true, 1)).IsSuccess);

        var badCoordinates = ValidDto(seed);
        badCoordinates.Location = new PointDto(500, 35);
        Assert.False((await service.CreateAsync(badCoordinates, true, 1)).IsSuccess);

        var badCapacity = ValidDto(seed);
        badCapacity.MaxConcurrentPets = -1;
        Assert.False((await service.CreateAsync(badCapacity, true, 1)).IsSuccess);

        Assert.Equal(0, await context.Clinics.CountAsync(g => g.ProviderId == seed.ProviderId));
    }

    [Fact]
    public async Task Update_ByNonOwner_IsDenied_AndByOwner_ResetsApproval()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        long id;
        using (var context = _fixture.NewContext())
            id = (await NewService(context).CreateAsync(ValidDto(seed), true, 1)).Data.Id;

        using var verify = _fixture.NewContext();
        var service = NewService(verify);
        var edit = ValidDto(seed, "Renamed");
        edit.Id = id;

        Assert.False((await service.UpdateAsync(edit, seed.OtherUserId)).IsSuccess);
        Assert.Equal("Salon", (await service.FindAsyncVDto(id)).Data.Name);

        Assert.True((await service.UpdateAsync(edit, seed.OwnerUserId)).IsSuccess);
        var after = (await NewService(_fixture.NewContext()).FindAsyncVDto(id)).Data;
        Assert.Equal("Renamed", after.Name);
        Assert.False(after.Approved);
        Assert.False(after.Active);
    }

    [Fact]
    public async Task Approve_Reject_RequiresReason_AndShowToSiteNeedsApprovedActive()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        using var context = _fixture.NewContext();
        var service = NewService(context);
        var id = (await service.CreateAsync(ValidDto(seed), byAdmin: false, actorUserId: seed.OwnerUserId)).Data.Id;

        Assert.False((await service.UpdateSiteVisibilityAsync(id, true)).IsSuccess);
        Assert.False((await service.UpdateActiveAsync(new ClinicActiveDto { Id = id, Active = true }, null)).IsSuccess);
        Assert.False((await service.UpdateApproveAsync(new ClinicApproveDto { Id = id, Approve = false })).IsSuccess);

        Assert.True((await service.UpdateApproveAsync(new ClinicApproveDto { Id = id, Approve = true })).IsSuccess);
        Assert.True((await service.UpdateSiteVisibilityAsync(id, true)).IsSuccess);

        Assert.True((await service.UpdateApproveAsync(new ClinicApproveDto { Id = id, Approve = false, ApprovalValue = "bad photos" })).IsSuccess);
        var rejected = (await NewService(_fixture.NewContext()).FindAsyncVDto(id)).Data;
        Assert.False(rejected.Approved);
        Assert.False(rejected.Active);
        Assert.False(rejected.ShowToSite);
        Assert.Equal("bad photos", rejected.ActivationValue);
    }

    [Fact]
    public async Task SoftDelete_HidesFromSearch_AndRestoreBringsBackInactive()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        using var context = _fixture.NewContext();
        var service = NewService(context);
        var id = (await service.CreateAsync(ValidDto(seed), true, 1)).Data.Id;

        Assert.False((await service.SoftDeleteAsync(id, seed.OtherUserId, seed.OtherUserId)).IsSuccess);
        Assert.True((await service.SoftDeleteAsync(id, seed.OwnerUserId, seed.OwnerUserId)).IsSuccess);

        using var verify = _fixture.NewContext();
        var reader = NewService(verify);
        Assert.Empty(reader.Search(new ClinicInputDto { ProviderId = seed.ProviderId }).List);
        Assert.Single(reader.Search(new ClinicInputDto { ProviderId = seed.ProviderId }, onlyDeleted: true).List);
        Assert.False((await reader.FindAsyncVDto(id)).IsSuccess);
        Assert.True((await reader.FindAsyncVDto(id, includeDeleted: true)).IsSuccess);

        Assert.True((await reader.RestoreAsync(id)).IsSuccess);
        var restored = (await NewService(_fixture.NewContext()).FindAsyncVDto(id)).Data;
        Assert.False(restored.Active);
        Assert.False(restored.ShowToSite);
    }

    [Fact]
    public async Task SetPets_ReplacesSetAndIgnoresDuplicates()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        using var context = _fixture.NewContext();
        var service = NewService(context);
        var id = (await service.CreateAsync(ValidDto(seed), true, 1)).Data.Id;

        Assert.True((await service.SetPetsAsync(id, new[] { seed.PetId, seed.PetId }, seed.OwnerUserId)).IsSuccess);
        Assert.Equal(new[] { seed.PetId }, (await service.FindAsyncVDto(id)).Data.PetIds);
        Assert.False((await service.SetPetsAsync(id, new[] { seed.PetId + 1000 }, seed.OwnerUserId)).IsSuccess);
        Assert.False((await service.SetPetsAsync(id, new[] { seed.PetId }, seed.OtherUserId)).IsSuccess);
        Assert.True((await service.SetPetsAsync(id, Array.Empty<long>(), null)).IsSuccess);
        Assert.Empty((await service.FindAsyncVDto(id)).Data.PetIds);
    }

    [Fact]
    public async Task AdminSettings_ValidateCommissionRange()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        using var context = _fixture.NewContext();
        var service = NewService(context);
        var id = (await service.CreateAsync(ValidDto(seed), true, 1)).Data.Id;

        Assert.False((await service.UpdateAdminSettingsAsync(new ClinicAdminSettingsDto { Id = id, CommissionPercent = 101 })).IsSuccess);
        Assert.True((await service.UpdateAdminSettingsAsync(new ClinicAdminSettingsDto { Id = id, CommissionPercent = 12.5m, Suggested = true })).IsSuccess);
        var view = (await NewService(_fixture.NewContext()).FindAsyncVDto(id)).Data;
        Assert.Equal(12.5m, view.CommissionPercent);
        Assert.True(view.Suggested);
    }
}
