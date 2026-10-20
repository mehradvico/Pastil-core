using Application.Common.Dto.Field;

namespace Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv.Dto
{
    public class ProviderRoleAssignmentDto : Id_FieldDto
    {
        public long ProviderId { get; set; }
        public long ProviderRoleId { get; set; }
        public bool Approved { get; set; }
    }
}
