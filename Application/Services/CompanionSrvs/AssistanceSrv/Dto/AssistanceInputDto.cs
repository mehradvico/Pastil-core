using Application.Common.Dto.Input;
using Application.Services.CompanionSrvs.AssistanceSrv.Iface;

namespace Application.Services.CompanionSrvs.AssistanceSrv.Dto
{
    public class AssistanceInputDto : BaseInputDto, IAssistanceSearchFields
    {
        public bool? IsPersonal { get; set; }
        public long? AssistanceGroupId { get; set; }
        public bool? ShowToSite { get; set; }
        /// <summary>true فقط خدمات گرومینگ (آرایشگاه)، false بدون آن‌ها؛ در endpointهای عمومی پیش‌فرض false است.</summary>
        public bool? IsGrooming { get; set; }
    }
}
