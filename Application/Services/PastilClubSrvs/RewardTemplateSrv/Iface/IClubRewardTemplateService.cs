using Application.Common.Dto.Result;
using Application.Services.PastilClubSrvs.RewardTemplateSrv.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.RewardTemplateSrv.Iface
{
    public interface IClubRewardTemplateService
    {
        Task<BaseResultDto<ClubRewardTemplateDto>> FindAsync(long id, CancellationToken cancellationToken = default);
        Task<ClubRewardTemplateSearchDto> SearchAsync(ClubRewardTemplateInputDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubRewardTemplateDto>> InsertAsync(ClubRewardTemplateDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<ClubRewardTemplateDto>> UpdateAsync(ClubRewardTemplateDto dto, CancellationToken cancellationToken = default);
    }
}
