using Application.Common.Dto.Input;
using Application.Services.ProviderSrvs.SubscriptionPlanSrv.Iface;

namespace Application.Services.ProviderSrvs.SubscriptionPlanSrv.Dto
{
    public class SubscriptionPlanInputDto : BaseInputDto, ISubscriptionPlanSearchFields
    {
        public bool? Active { get; set; }
    }
}
