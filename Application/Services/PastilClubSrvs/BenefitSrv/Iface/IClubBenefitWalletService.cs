using Application.Common.Dto.Result;
using Application.Services.PastilClubSrvs.BenefitSrv.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.BenefitSrv.Iface
{
    public interface IClubBenefitWalletService
    {
        Task<BaseResultDto<ClubBenefitWalletVDto>> GetAsync(
            long userId,
            bool includeConsumed,
            CancellationToken cancellationToken = default);
    }
}
