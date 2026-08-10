using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.RewardOfferSrv.Iface
{
    public interface IClubRewardEligibilityService
    {
        Task<bool> IsPetEligibleAsync(long userId, long rewardTemplateId, CancellationToken cancellationToken = default);
    }
}
