using Application.Common.Dto.Field;

namespace Application.Services.ProviderSrvs.ProviderRoleSrv.Dto
{
    public class ProviderRoleVDto : Name_FieldDto
    {
        public string Label { get; set; }
        public bool Active { get; set; }
    }
}
