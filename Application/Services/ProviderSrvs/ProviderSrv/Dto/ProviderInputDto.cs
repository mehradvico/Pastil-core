using Application.Common.Dto.Input;
using Application.Services.ProviderSrvs.ProviderSrv.Iface;

namespace Application.Services.ProviderSrvs.ProviderSrv.Dto
{
    public class ProviderInputDto : BaseInputDto, IProviderSearchFields
    {
        public bool? Active { get; set; }
        public bool? Approved { get; set; }
        /// <summary>فقط ارائه‌دهندگانی که الان اشتراک این پلن را دارند.</summary>
        public long? PlanId { get; set; }
        /// <summary>فقط ارائه‌دهندگانی که این نقش (ProviderRole) را دارند.</summary>
        public long? ProviderRoleId { get; set; }
    }
}
