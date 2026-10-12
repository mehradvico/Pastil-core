using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.PermissionSrv.Dto;
using Entities.Entities.Security;

namespace Application.Services.Accounting.PermissionSrv.Iface
{
    public interface IPermissionService : ICommonSrv<Permission, PermissionDto>
    {
        PermissionSearchDto Search(PermissionInputDto searchDto);
        BaseResultDto Menu();
        BaseResultDto MyAdminActions();
    }
}
