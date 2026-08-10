using Application.Common.Dto.Result;
using Application.Services.PastilClubSrvs.RewardOfferSrv.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.RewardOfferSrv.Iface
{
    public interface IClubRewardOfferService
    {
        Task<BaseResultDto<ClubRewardOfferVDto>> FindAdminAsync(long id, CancellationToken cancellationToken = default);
        Task<ClubRewardOfferSearchDto> SearchAdminAsync(ClubRewardOfferInputDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubRewardOfferVDto>> CreateManualAsync(ClubRewardOfferCreateDto dto, long adminId, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubRewardOfferVDto>> ApproveAsync(long offerId, long adminId, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubRewardOfferVDto>> RejectAsync(long offerId, string reason, long adminId, CancellationToken cancellationToken = default);
        Task<BaseResultDto> BulkApproveAsync(ClubRewardOfferBulkDecisionDto dto, long adminId, CancellationToken cancellationToken = default);
        Task<BaseResultDto> BulkRejectAsync(ClubRewardOfferBulkDecisionDto dto, long adminId, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubRewardOfferVDto>> FindUserAsync(long id, long userId, CancellationToken cancellationToken = default);
        Task<ClubRewardOfferSearchDto> SearchUserAsync(ClubRewardOfferInputDto dto, long userId, CancellationToken cancellationToken = default);
    }
}
