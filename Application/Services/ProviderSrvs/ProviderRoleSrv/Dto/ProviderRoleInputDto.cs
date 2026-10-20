using Application.Common.Dto.Input;
using Application.Services.ProviderSrvs.ProviderRoleSrv.Iface;

namespace Application.Services.ProviderSrvs.ProviderRoleSrv.Dto
{
    public class ProviderRoleInputDto : BaseInputDto, IProviderRoleSearchFields
    {
        public bool? Active { get; set; }
    }
}
