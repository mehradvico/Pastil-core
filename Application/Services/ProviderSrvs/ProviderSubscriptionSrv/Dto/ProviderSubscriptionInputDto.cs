using Application.Common.Dto.Input;
using Application.Services.ProviderSrvs.ProviderSubscriptionSrv.Iface;

namespace Application.Services.ProviderSrvs.ProviderSubscriptionSrv.Dto
{
    public class ProviderSubscriptionInputDto : BaseInputDto, IProviderSubscriptionSearchFields
    {
        public long? ProviderId { get; set; }
        public long? PlanId { get; set; }
        /// <summary>true = فقط اشتراک‌های جاری.</summary>
        public bool? ActiveNow { get; set; }
    }
}
