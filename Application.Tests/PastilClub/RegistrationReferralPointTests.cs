using Application.Common.Dto.Result;
using Application.Services.PastilClubSrvs.PointEventSrv;
using Application.Services.PastilClubSrvs.PointEventSrv.Dto;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Application.Services.PastilClubSrvs.PointSrv.Dto;
using Entities.Entities.PastilClubField;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Application.Tests.PastilClub;

public class RegistrationReferralPointTests
{
    [Fact]
    public async Task UserReferral_AwardsReferrerAndNewUser()
    {
        var pointEventService = new CapturingPointEventService();
        var service = new ClubPointIntegrationService(
            pointEventService,
            NullLogger<ClubPointIntegrationService>.Instance);

        await service.RegistrationReferralCompletedAsync(20, 10, false);

        Assert.Collection(
            pointEventService.Events,
            item =>
            {
                Assert.Equal(10, item.UserId);
                Assert.Equal(ClubPointEventTypeEnum.UserReferralReferrer, item.EventType);
                Assert.Equal(ClubPointSourceTypeEnum.UserReferral, item.SourceType);
                Assert.Equal(20, item.SourceId);
            },
            item =>
            {
                Assert.Equal(20, item.UserId);
                Assert.Equal(ClubPointEventTypeEnum.UserReferralReferee, item.EventType);
                Assert.Equal(ClubPointSourceTypeEnum.UserReferral, item.SourceType);
                Assert.Equal(20, item.SourceId);
            });
    }

    [Fact]
    public async Task BusinessReferral_AwardsOwnerAndNewUser()
    {
        var pointEventService = new CapturingPointEventService();
        var service = new ClubPointIntegrationService(
            pointEventService,
            NullLogger<ClubPointIntegrationService>.Instance);

        await service.RegistrationReferralCompletedAsync(20, 10, true);

        Assert.Collection(
            pointEventService.Events,
            item =>
            {
                Assert.Equal(10, item.UserId);
                Assert.Equal(ClubPointEventTypeEnum.UserReferralReferrer, item.EventType);
                Assert.Equal(ClubPointSourceTypeEnum.BusinessReferral, item.SourceType);
            },
            item =>
            {
                Assert.Equal(20, item.UserId);
                Assert.Equal(ClubPointEventTypeEnum.BusinessReferralUser, item.EventType);
                Assert.Equal(ClubPointSourceTypeEnum.BusinessReferral, item.SourceType);
            });
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(20, 0)]
    [InlineData(20, 20)]
    public async Task InvalidReferral_DoesNotAward(long newUserId, long referrerUserId)
    {
        var pointEventService = new CapturingPointEventService();
        var service = new ClubPointIntegrationService(
            pointEventService,
            NullLogger<ClubPointIntegrationService>.Instance);

        await service.RegistrationReferralCompletedAsync(newUserId, referrerUserId, false);

        Assert.Empty(pointEventService.Events);
    }

    private sealed class CapturingPointEventService : IClubPointEventService
    {
        public List<ClubPointEventDto> Events { get; } = [];

        public Task<BaseResultDto<ClubPointTransactionVDto>> AwardAsync(
            ClubPointEventDto dto,
            CancellationToken cancellationToken = default)
        {
            Events.Add(dto);
            return Task.FromResult(new BaseResultDto<ClubPointTransactionVDto>(true, null));
        }

        public Task<BaseResultDto<ClubPointTransactionVDto>> ReverseAsync(
            ClubPointEventDto dto,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new BaseResultDto<ClubPointTransactionVDto>(true, null));
    }
}
