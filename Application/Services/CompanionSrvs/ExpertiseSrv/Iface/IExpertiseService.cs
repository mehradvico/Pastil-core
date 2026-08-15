using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.ExpertiseSrv.Dto;
using Entities.Entities.CompanionField;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.ExpertiseSrv.Iface
{
    public interface IExpertiseService : ICommonSrv<Expertise, ExpertiseDto>
    {
        ExpertiseSearchDto Search(ExpertiseInputDto dto);
        Task<BaseResultDto<ExpertiseVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto<ExpertiseDto>> InsertValidatedAsync(ExpertiseDto dto);
        Task<BaseResultDto> UpdateValidatedAsync(ExpertiseDto dto);
        Task<BaseResultDto> DeleteValidatedAsync(long id);
    }
}
