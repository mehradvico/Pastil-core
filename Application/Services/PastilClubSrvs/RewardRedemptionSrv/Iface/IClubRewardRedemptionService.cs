using Application.Common.Dto.Result;
using Application.Services.PastilClubSrvs.RewardRedemptionSrv.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.RewardRedemptionSrv.Iface
{
    public interface IClubRewardRedemptionService
    {
        Task<BaseResultDto<ClubRewardRedemptionVDto>> RedeemAsync(long userId, long offerId, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubRewardRedemptionVDto>> FindAdminAsync(long id, CancellationToken cancellationToken = default);
        Task<ClubRewardRedemptionSearchDto> SearchAdminAsync(ClubRewardRedemptionInputDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubRewardRedemptionVDto>> FindUserAsync(long id, long userId, CancellationToken cancellationToken = default);
        Task<ClubRewardRedemptionSearchDto> SearchUserAsync(ClubRewardRedemptionInputDto dto, long userId, CancellationToken cancellationToken = default);
    }
}
