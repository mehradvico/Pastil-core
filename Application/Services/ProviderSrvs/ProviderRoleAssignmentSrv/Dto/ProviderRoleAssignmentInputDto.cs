using Application.Common.Dto.Input;
using Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv.Iface;

namespace Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv.Dto
{
    public class ProviderRoleAssignmentInputDto : BaseInputDto, IProviderRoleAssignmentSearchFields
    {
        public long? ProviderId { get; set; }
        public long? ProviderRoleId { get; set; }
        public bool? Approved { get; set; }
    }
}
