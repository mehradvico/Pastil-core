using Application.Common.Dto.Result;
using Application.Services.CoachSrvs;
using Application.Services.CompanionSrvs.ConsultationOnlineSrv;
using Entities.Entities.CoachField;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Application.Tests.Coach;

/// <summary>«من آنلاینم» مربی: نگاشت شناسه‌ی مربی به Companion قدیمی و کنترل مالکیت.</summary>
public class CoachConsultationOnlineTests : IClassFixture<CoachLocalDbFixture>
{
    private readonly CoachLocalDbFixture _fixture;

    public CoachConsultationOnlineTests(CoachLocalDbFixture fixture) => _fixture = fixture;

    private sealed class RecordingOnline : IConsultationOnlineService
    {
        public long? LastCompanionId;
        public bool? LastOnline;

        public Task<BaseResultDto<ConsultationOnlineStatusDto>> GetMineAsync(long userId, long? companionId)
        {
            LastCompanionId = companionId;
            return Task.FromResult(new BaseResultDto<ConsultationOnlineStatusDto>(true, new ConsultationOnlineStatusDto()));
        }

        public Task<BaseResultDto<ConsultationOnlineStatusDto>> SetMineAsync(long userId, long? companionId, bool online)
        {
            LastCompanionId = companionId;
            LastOnline = online;
            return Task.FromResult(new BaseResultDto<ConsultationOnlineStatusDto>(true, new ConsultationOnlineStatusDto { Online = online }));
        }

        public Task<ConsultationOnlineStatusDto> GetPublicAsync(long companionId) => Task.FromResult(new ConsultationOnlineStatusDto());
    }

    private async Task<long> AddCoachAsync(long ownerId, long? legacyCompanionId)
    {
        using var context = _fixture.NewContext();
        var seed = _fixture.Seed();
        var coach = new Entities.Entities.CoachField.Coach
        {
            Name = "Online", ProviderId = seed.ProviderId, OwnerId = ownerId, LegacyCompanionId = legacyCompanionId, Phone = "021",
            AddressValue = "a", StateId = seed.StateId, CityId = seed.CityId, Active = true, Approved = true,
        };
        context.Coaches.Add(coach);
        await context.SaveChangesAsync();
        return coach.Id;
    }

    [Fact]
    public async Task Set_MapsToLegacyCompanion_AndRejectsStrangersAndUnlinkedCoaches()
    {
        if (!_fixture.Available) return;
        var seed = _fixture.Seed();
        var fake = new RecordingOnline();
        var service = new CoachConsultationOnlineService(_fixture.NewContext(), fake);

        var linked = await AddCoachAsync(seed.OwnerUserId, 777);
        var ok = await service.SetMineAsync(seed.OwnerUserId, linked, true);
        Assert.True(ok.IsSuccess);
        Assert.Equal(777, fake.LastCompanionId);
        Assert.True(fake.LastOnline);

        // غریبه
        fake.LastCompanionId = null;
        Assert.False((await service.SetMineAsync(seed.OtherUserId, linked, true)).IsSuccess);
        Assert.Null(fake.LastCompanionId);

        // مربی بدون Companion قدیمی
        var unlinked = await AddCoachAsync(seed.OwnerUserId, null);
        Assert.False((await service.SetMineAsync(seed.OwnerUserId, unlinked, true)).IsSuccess);
        Assert.False((await service.GetMineAsync(seed.OwnerUserId, unlinked)).IsSuccess);
        Assert.Null(fake.LastCompanionId);
    }
}
