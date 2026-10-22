using Application.Common.Dto.Field;

namespace Application.Services.CompanionSrvs.ExpertiseSrv.Dto
{
    public class ExpertiseVDto : Name_FieldDto
    {
        public int Priority { get; set; }
        public bool Active { get; set; }
        public long? ProviderRoleId { get; set; }
        public string ProviderRoleName { get; set; }
        public string ProviderRoleLabel { get; set; }
    }
}
