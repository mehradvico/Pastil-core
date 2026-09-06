using Application.Common.Dto.Field;

namespace Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Dto
{
    public class CompanionAssistancePackageOnlineSelectionActivationDto : Id_FieldDto
    {
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
    }
}
