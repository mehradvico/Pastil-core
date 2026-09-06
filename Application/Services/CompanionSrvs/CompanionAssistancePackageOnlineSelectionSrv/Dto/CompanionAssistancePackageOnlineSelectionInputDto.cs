using Application.Common.Dto.Input;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Iface;

namespace Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Dto
{
    public class CompanionAssistancePackageOnlineSelectionInputDto : BaseInputDto, ICompanionAssistancePackageOnlineSelectionSearchFields
    {
        public long? CompanionAssistancePackageId { get; set; }
    }
}
