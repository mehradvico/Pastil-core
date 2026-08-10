using Application.Services.PastilClubSrvs.BenefitSrv;
using Entities.Entities.PastilClubField;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.BenefitSrv.Iface
{
    public interface IClubRewardBenefitFactory
    {
        Task<ClubRewardBenefitResult> CreateAsync(
            ClubRewardRedemption redemption,
            ClubRewardTemplate template,
            CancellationToken cancellationToken = default);
    }
}
