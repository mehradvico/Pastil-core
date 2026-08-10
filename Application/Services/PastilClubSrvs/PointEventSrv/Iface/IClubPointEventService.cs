using Application.Common.Dto.Result;
using Application.Services.PastilClubSrvs.PointEventSrv.Dto;
using Application.Services.PastilClubSrvs.PointSrv.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.PointEventSrv.Iface
{
    public interface IClubPointEventService
    {
        Task<BaseResultDto<ClubPointTransactionVDto>> AwardAsync(
            ClubPointEventDto dto,
            CancellationToken cancellationToken = default);

        Task<BaseResultDto<ClubPointTransactionVDto>> ReverseAsync(
            ClubPointEventDto dto,
            CancellationToken cancellationToken = default);
    }
}
