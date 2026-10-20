using Application.Common.Interface;
using Application.Services.ProviderSrvs.ProviderRoleSrv.Dto;
using Entities.Entities.ProviderField;

namespace Application.Services.ProviderSrvs.ProviderRoleSrv.Iface
{
    public interface IProviderRoleService : ICommonSrv<ProviderRole, ProviderRoleDto>
    {
        ProviderRoleSearchDto Search(ProviderRoleInputDto input);
    }
}
