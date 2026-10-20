using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderRoleSrv.Iface;
using AutoMapper;
using Entities.Entities.ProviderField;
using System.Linq;

namespace Application.Services.ProviderSrvs.ProviderRoleSrv.Dto
{
    public class ProviderRoleSearchDto : BaseSearchDto<ProviderRole, ProviderRoleVDto>, IProviderRoleSearchFields
    {
        public ProviderRoleSearchDto(ProviderRoleInputDto dto, IQueryable<ProviderRole> list, IMapper mapper) : base(dto, list, mapper)
        {
            Active = dto.Active;
        }

        public bool? Active { get; set; }
    }
}
