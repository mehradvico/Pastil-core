using Application.Common.Dto.Input;
using Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv.Iface;

namespace Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv.Dto
{
    public class CompanionInstantCallRequestInputDto : BaseInputDto, ICompanionInstantCallRequestSearchFields
    {
        public long? CompanionAssistancePackageOnlineSelectionId { get; set; }
    }
}
