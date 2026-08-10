using Application.Common.Dto.Result;
using Application.Services.PastilClubSrvs.PointSrv.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.PointSrv.Iface
{
    public interface IClubPointRuleService
    {
        Task<BaseResultDto<ClubPointRuleDto>> FindAsync(long id, CancellationToken cancellationToken = default);
        Task<ClubPointRuleSearchDto> SearchAsync(ClubPointRuleInputDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubPointRuleDto>> InsertAsync(ClubPointRuleDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubPointRuleDto>> UpdateAsync(ClubPointRuleDto dto, CancellationToken cancellationToken = default);
    }
}
