using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.SubscriptionPlanSrv.Iface;
using AutoMapper;
using Entities.Entities.ProviderField;
using System.Linq;

namespace Application.Services.ProviderSrvs.SubscriptionPlanSrv.Dto
{
    public class SubscriptionPlanSearchDto : BaseSearchDto<SubscriptionPlan, SubscriptionPlanVDto>, ISubscriptionPlanSearchFields
    {
        public SubscriptionPlanSearchDto(SubscriptionPlanInputDto dto, IQueryable<SubscriptionPlan> list, IMapper mapper) : base(dto, list, mapper)
        {
            Active = dto.Active;
        }

        public bool? Active { get; set; }
    }
}
