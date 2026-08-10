using Application.Common.Dto.Result;
using Application.Services.PastilClubSrvs.PointSrv.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.PointSrv.Iface
{
    public interface IClubPointService
    {
        Task<BaseResultDto<ClubPointBalanceVDto>> GetBalanceAsync(long userId, CancellationToken cancellationToken = default);
        Task<ClubPointTransactionSearchDto> SearchTransactionsAsync(ClubPointTransactionInputDto dto, long? userId = null, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubPointTransactionVDto>> EarnAsync(ClubPointChangeDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubPointTransactionVDto>> SpendAsync(ClubPointChangeDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubPointTransactionVDto>> ReverseEarnAsync(ClubPointChangeDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubPointTransactionVDto>> IncreaseManualAsync(ClubManualPointDto dto, long adminId, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubPointTransactionVDto>> DecreaseManualAsync(ClubManualPointDto dto, long adminId, CancellationToken cancellationToken = default);
    }
}
