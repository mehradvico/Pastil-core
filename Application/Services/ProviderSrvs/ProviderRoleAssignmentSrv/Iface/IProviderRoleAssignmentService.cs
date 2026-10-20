using Application.Common.Interface;
using Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv.Dto;
using Entities.Entities.ProviderField;

namespace Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv.Iface
{
    public interface IProviderRoleAssignmentService : ICommonSrv<ProviderRoleAssignment, ProviderRoleAssignmentDto>
    {
        ProviderRoleAssignmentSearchDto Search(ProviderRoleAssignmentInputDto input);
    }
}
