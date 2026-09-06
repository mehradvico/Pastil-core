using Application.Common.Dto.Field;

namespace Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSrv.Dto
{
    public class CompanionAssistancePackageOnlineVDto : Name_FieldDto
    {
        public double Price { get; set; }
        public bool Active { get; set; }
        public bool IsInstant { get; set; }
    }
}
