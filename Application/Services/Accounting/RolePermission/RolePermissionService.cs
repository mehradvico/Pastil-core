using Application.Common.Dto.Result;
using Application.Services.Accounting.RolePermission.Dto;
using Application.Services.Accounting.RolePermission.Iface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Accounting.RolePermission
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public RolePermissionService(IDataBaseContext _context, IMapper mapper)
        {
            this._context = _context;
            this.mapper = mapper;

        }
        public async Task<BaseResultDto> FindAsyncDto(long id)
        {
            var rolePermission = await _context.Roles.Include(s => s.Permissions).FirstOrDefaultAsync(s => s.Id.Equals(id));
            if (rolePermission == null)
            {
                return new BaseResultDto(false, val: Resource.Notification.ResourceNotFind);
            }
            else
            {
                return new BaseResultDto<RolePermissionDto>(true, mapper.Map<RolePermissionDto>(rolePermission));

            }
        }

        public async Task<BaseResultDto> InsertAndUpdateAsyncDto(RolePermissionDto dto)
        {
            try
            {
                if (dto != null)
                {
                    var role = await _context.Roles.AsTracking().Include(s => s.Permissions).FirstOrDefaultAsync(s => s.Id.Equals(dto.RoleDto.Id));
                    if (role != null)
                    {
                        foreach (var permission in role.Permissions)
                        {
                            if (!dto.PermissionsDto.Any(s => s.Id == permission.Id))
                            {
                                role.Permissions.Remove(permission);
                            }
                        }
                        foreach (var permissionsDto in dto.PermissionsDto)
                        {
                            if (!role.Permissions.Any(s => s.Id == permissionsDto.Id))
                            {
                                var perEntity = _context.Permissions.FirstOrDefault(s => s.Id == permissionsDto.Id);
                                if (perEntity != null)
                                    role.Permissions.Add(perEntity);
                            }
                        }
                        await _context.SaveChangesAsync();
                        return new BaseResultDto(true);
                    }
                }
                return new BaseResultDto(false, val: Resource.Notification.ResourceNotFind);

            }
            catch (Exception e)
            {
                return new BaseResultDto(false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(e));
            }
        }
    }
}
