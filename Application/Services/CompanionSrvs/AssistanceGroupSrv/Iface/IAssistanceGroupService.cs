using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.AssistanceGroupSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.AssistanceGroupSrv.Iface
{
    public interface IAssistanceGroupService :
        ICommonSrv<AssistanceGroup, AssistanceGroupDto>
    {
        AssistanceGroupSearchDto Search(AssistanceGroupInputDto searchDto);
        Task<BaseResultDto<AssistanceGroupVDto>> FindAsyncVDto(long id);
    }
}
