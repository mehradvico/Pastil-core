using Application.Common.Dto.LocationPoint;
using Application.Services.CoachSrvs.CoachProfileSrv;
using Application.Services.CoachSrvs.CoachProfileSrv.Dto;
using Application.Services.CoachSrvs.CoachSrv;
using Application.Services.CoachSrvs.CoachSrv.Dto;
using Entities.Entities;
using Entities.Entities.LocationField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Application.Tests.Coach;

public class CoachProfileServiceTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public CoachProfileServiceTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private CoachProfileService NewService() =>
        new(_fixture.NewContext(), _fixture.Mapper, NullLogger<CoachProfileService>.Instance);

    private async Task<(long CoachId, CoachLocalDbFixture.SeedData Seed, long WeekDayId)> SetupAsync()
    {
        var seed = _fixture.Seed();
        using var context = _fixture.NewContext();
        var weekDay = new WeekDay { Name = "Sat", Label = "sat", Number = 1 };
        context.WeekDays.Add(weekDay);
        await context.SaveChangesAsync();
        var core = new CoachCoreService(context, _fixture.Mapper, NullLogger<CoachCoreService>.Instance);
        var coach = await core.CreateAsync(new CoachDto
        {
            Name = "Salon", ProviderId = seed.ProviderId, Phone = "021", AddressValue = "a",
            StateId = seed.StateId, CityId = seed.CityId, Location = new PointDto(51, 35),
        }, true, 1);
        return (coach.Data.Id, seed, weekDay.Id);
    }

    [Fact]
    public async Task Staff_AddByMobile_RejectsOwnerDuplicatesUnknown_AndKeepsCount()
    {
        if (!_fixture.Available) return;
        var (coachId, seed, _) = await SetupAsync();

        Assert.False((await NewService().AddStaffAsync(new CoachStaffAddDto { CoachId = coachId, Mobile = seed.OtherMobile }, seed.OtherUserId)).IsSuccess);
        Assert.False((await NewService().AddStaffAsync(new CoachStaffAddDto { CoachId = coachId, Mobile = "abc" }, seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().AddStaffAsync(new CoachStaffAddDto { CoachId = coachId, Mobile = "09129999999" }, seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().AddStaffAsync(new CoachStaffAddDto { CoachId = coachId, Mobile = seed.OwnerMobile }, seed.OwnerUserId)).IsSuccess);

        // موبایل با +98 و ارقام فارسی هم پذیرفته می‌شود (نرمال‌سازی)
        var added = await NewService().AddStaffAsync(new CoachStaffAddDto { CoachId = coachId, Mobile = "+98 " + seed.OtherMobile.Substring(1) }, seed.OwnerUserId);
        Assert.True(added.IsSuccess);
        Assert.False((await NewService().AddStaffAsync(new CoachStaffAddDto { CoachId = coachId, Mobile = seed.OtherMobile }, seed.OwnerUserId)).IsSuccess);

        // تا پذیرش دعوت توسط خود کاربر، پرسنل شمرده نمی‌شود
        using (var context = _fixture.NewContext())
            Assert.Equal(0, (await context.Coaches.AsNoTracking().FirstAsync(g => g.Id == coachId)).StaffCount);
        Assert.Single(await NewService().ListInvitesAsync(seed.OtherUserId));
        Assert.False((await NewService().RespondInviteAsync(added.Data.Id, seed.OwnerUserId, true)).IsSuccess);
        Assert.True((await NewService().RespondInviteAsync(added.Data.Id, seed.OtherUserId, true)).IsSuccess);
        Assert.False((await NewService().RespondInviteAsync(added.Data.Id, seed.OtherUserId, false)).IsSuccess);
        using (var context = _fixture.NewContext())
            Assert.Equal(1, (await context.Coaches.AsNoTracking().FirstAsync(g => g.Id == coachId)).StaffCount);

        Assert.True((await NewService().SetStaffActiveAsync(added.Data.Id, false, seed.OwnerUserId)).IsSuccess);
        using (var context = _fixture.NewContext())
            Assert.Equal(0, (await context.Coaches.AsNoTracking().FirstAsync(g => g.Id == coachId)).StaffCount);

        Assert.False((await NewService().RemoveStaffAsync(added.Data.Id, seed.OtherUserId)).IsSuccess);
        Assert.True((await NewService().RemoveStaffAsync(added.Data.Id, seed.OwnerUserId)).IsSuccess);
        Assert.Empty((await NewService().ListStaffAsync(coachId, seed.OwnerUserId)).Data);
    }

    [Fact]
    public async Task Times_ValidateFormatOrderOverlapAndReplaceAll()
    {
        if (!_fixture.Available) return;
        var (coachId, seed, day) = await SetupAsync();
        CoachTimeInputDto T(string s, string e, int cap = 2, bool active = true) => new() { WeekDayId = day, StartTime = s, EndTime = e, Capacity = cap, Active = active };

        Assert.False((await NewService().SetTimesAsync(coachId, new[] { T("9", "17") }, seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().SetTimesAsync(coachId, new[] { T("17:00", "09:00") }, seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().SetTimesAsync(coachId, new[] { T("09:00", "24:30") }, seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().SetTimesAsync(coachId, new[] { T("09:00", "12:00", 0) }, seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().SetTimesAsync(coachId, new[] { T("09:00", "12:00"), T("11:00", "15:00") }, seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().SetTimesAsync(coachId, new[] { T("09:00", "12:00") }, seed.OtherUserId)).IsSuccess);
        Assert.False((await NewService().SetTimesAsync(coachId, new[] { new CoachTimeInputDto { WeekDayId = day + 500, StartTime = "09:00", EndTime = "10:00", Capacity = 1 } }, seed.OwnerUserId)).IsSuccess);

        // همپوشانی با ردیف غیرفعال مشکلی ندارد؛ ردیف‌های پشت‌سرهم (۱۲ تا ۱۲) هم مجازند
        var ok = await NewService().SetTimesAsync(coachId, new[] { T("14:00", "18:00"), T("09:00", "12:00"), T("12:00", "13:00"), T("10:00", "11:00", active: false) }, seed.OwnerUserId);
        Assert.True(ok.IsSuccess);
        Assert.Equal(4, ok.Data.Count);

        var replaced = await NewService().SetTimesAsync(coachId, new[] { T("08:00", "20:00", 5) }, null);
        Assert.True(replaced.IsSuccess);
        Assert.Single(replaced.Data);
        Assert.Equal(5, replaced.Data[0].Capacity);
    }

    [Fact]
    public async Task Zones_ValidateCityInStateAndReplaceSet()
    {
        if (!_fixture.Available) return;
        var (coachId, seed, _) = await SetupAsync();
        Assert.False((await NewService().SetZonesAsync(coachId, new[] { new CoachZoneInputDto { StateId = seed.StateId, CityId = seed.CityId + 500 } }, seed.OwnerUserId)).IsSuccess);
        Assert.False((await NewService().SetZonesAsync(coachId, new[] { new CoachZoneInputDto { StateId = seed.StateId, CityId = seed.CityId } }, seed.OtherUserId)).IsSuccess);

        var set = await NewService().SetZonesAsync(coachId,
            new[] { new CoachZoneInputDto { StateId = seed.StateId, CityId = seed.CityId }, new CoachZoneInputDto { StateId = seed.StateId, CityId = seed.CityId } }, seed.OwnerUserId);
        Assert.True(set.IsSuccess);
        Assert.Single(set.Data);
        Assert.Empty((await NewService().SetZonesAsync(coachId, Array.Empty<CoachZoneInputDto>(), seed.OwnerUserId)).Data);
        Assert.Single((await NewService().SetZonesAsync(coachId, new[] { new CoachZoneInputDto { StateId = seed.StateId, CityId = seed.CityId } }, null)).Data);
    }

    [Fact]
    public async Task Gallery_AddRemoveWithOwnershipAndNoDuplicates()
    {
        if (!_fixture.Available) return;
        var (coachId, seed, _) = await SetupAsync();
        var add = new CoachPictureAddDto { CoachId = coachId, PictureId = seed.PictureIds[0], Label = "front" };

        Assert.False((await NewService().AddPictureAsync(add, seed.OtherUserId)).IsSuccess);
        Assert.False((await NewService().AddPictureAsync(new CoachPictureAddDto { CoachId = coachId, PictureId = 98765 }, seed.OwnerUserId)).IsSuccess);
        var added = await NewService().AddPictureAsync(add, seed.OwnerUserId);
        Assert.True(added.IsSuccess);
        Assert.False((await NewService().AddPictureAsync(add, seed.OwnerUserId)).IsSuccess);
        Assert.Single((await NewService().GetPicturesAsync(coachId)).Data);

        Assert.False((await NewService().RemovePictureAsync(added.Data.Id, seed.OtherUserId)).IsSuccess);
        Assert.True((await NewService().RemovePictureAsync(added.Data.Id, seed.OwnerUserId)).IsSuccess);
        Assert.Empty((await NewService().GetPicturesAsync(coachId)).Data);
    }
}
