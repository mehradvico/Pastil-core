using Application.Common.Dto.Field;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSrv.Dto;

namespace Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSelectionSrv.Dto
{
    public class CompanionAssistancePackageOnlineSelectionVDto : Id_FieldDto
    {
        public long CompanionAssistancePackageId { get; set; }
        public long CompanionAssistancePackageOnlineId { get; set; }
        public double Price { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }

        public CompanionAssistancePackageVDto CompanionAssistancePackage { get; set; }
        public CompanionAssistancePackageOnlineVDto CompanionAssistancePackageOnline { get; set; }
    }
}
