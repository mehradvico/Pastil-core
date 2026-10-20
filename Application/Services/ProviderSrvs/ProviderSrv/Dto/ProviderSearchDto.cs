using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderSrv.Iface;
using AutoMapper;
using Entities.Entities.ProviderField;
using System.Linq;

namespace Application.Services.ProviderSrvs.ProviderSrv.Dto
{
    public class ProviderSearchDto : BaseSearchDto<Provider, ProviderVDto>, IProviderSearchFields
    {
        public ProviderSearchDto(ProviderInputDto dto, IQueryable<Provider> list, IMapper mapper) : base(dto, list, mapper)
        {
            Active = dto.Active;
            Approved = dto.Approved;
            PlanId = dto.PlanId;
            ProviderRoleId = dto.ProviderRoleId;
        }

        public bool? Active { get; set; }
        public bool? Approved { get; set; }
        public long? PlanId { get; set; }
        public long? ProviderRoleId { get; set; }
    }
}
