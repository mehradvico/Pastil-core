using Application.Common.Dto.Input;
using Application.Services.CompanionSrvs.AssistanceSrv.Iface;

namespace Application.Services.CompanionSrvs.AssistanceSrv.Dto
{
    public class AssistanceInputDto : BaseInputDto, IAssistanceSearchFields
    {
        public bool? IsPersonal { get; set; }
        public long? AssistanceGroupId { get; set; }
    }
}
