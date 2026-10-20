using Application.Common.Dto.Field;
using Application.Services.ProviderSrvs.ProviderRoleSrv.Dto;

namespace Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv.Dto
{
    public class ProviderRoleAssignmentVDto : Id_FieldDto
    {
        public long ProviderId { get; set; }
        public long ProviderRoleId { get; set; }
        public bool Approved { get; set; }

        public ProviderRoleVDto ProviderRole { get; set; }
    }
}
