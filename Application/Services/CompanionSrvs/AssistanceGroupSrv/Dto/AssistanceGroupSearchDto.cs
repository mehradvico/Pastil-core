using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.AssistanceGroupSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CompanionSrvs.AssistanceGroupSrv.Dto
{
    public class AssistanceGroupSearchDto :
        BaseSearchDto<AssistanceGroup, AssistanceGroupVDto>,
        IAssistanceGroupSearchFields
    {
        public AssistanceGroupSearchDto(
            AssistanceGroupInputDto dto,
            IQueryable<AssistanceGroup> list,
            IMapper mapper)
            : base(dto, list, mapper)
        {
        }
    }
}
