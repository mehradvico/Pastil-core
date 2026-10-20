using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderSubscriptionSrv.Iface;
using AutoMapper;
using Entities.Entities.ProviderField;
using System.Linq;

namespace Application.Services.ProviderSrvs.ProviderSubscriptionSrv.Dto
{
    public class ProviderSubscriptionSearchDto : BaseSearchDto<ProviderSubscription, ProviderSubscriptionVDto>, IProviderSubscriptionSearchFields
    {
        public ProviderSubscriptionSearchDto(ProviderSubscriptionInputDto dto, IQueryable<ProviderSubscription> list, IMapper mapper) : base(dto, list, mapper)
        {
            ProviderId = dto.ProviderId;
            PlanId = dto.PlanId;
            ActiveNow = dto.ActiveNow;
        }

        public long? ProviderId { get; set; }
        public long? PlanId { get; set; }
        public bool? ActiveNow { get; set; }
    }
}
