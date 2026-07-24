using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.Accounting.PermissionSrv.Dto;
using Application.Services.Accounting.PermissionSrv.Iface;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.Dto;
using AutoMapper;
using Entities.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.PermissionSrv
{
    public class PermissionService : CommonSrv<Permission, PermissionDto>, IPermissionService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly CurrentUserDto _currentUser;
        public PermissionService(IDataBaseContext _context, IMapper mapper, ICurrentUserHelper currentUserHelper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._currentUser = currentUserHelper.CurrentUser;
        }
        private IQueryable<Permission> BaseSaerch(PermissionInputDto searchDto)
        {
            var query = _context.Permissions.Include(s => s.Children).ThenInclude(s => s.Children).ThenInclude(s => s.Children).AsQueryable();
            query = query.Where(s => s.ParentId == searchDto.ParentId);
            if (searchDto.RoleId.HasValue)
            {
                query = query.Where(s => s.Roles.Any(s => s.Id.Equals(searchDto.RoleId)));
            }
            if (!string.IsNullOrEmpty(searchDto.Q))
            {
                query = query.Where(s => s.Name.Contains(searchDto.Q));
            }
            switch (searchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    {
                        query = query.OrderBy(s => s.Priority);
                        break;
                    }
                default:
                    break;
            }
            return query;
        }
        public PermissionSearchDto Search(PermissionInputDto searchDto)
        {
            var query = BaseSaerch(searchDto);
            return new PermissionSearchDto(searchDto, query, mapper);
        }


        public BaseResultDto Menu()
        {
            var query = _context.Permissions
                .Where(s => s.ParentId == null)
                .OrderBy(s => s.Priority)
                .AsQueryable();
            if (_currentUser.RoleEnum == RoleEnum.Admin.ToString())
            {
                query = query
                    .Include(s => s.Children
                        .Where(x => x.IsMenu)
                        .OrderBy(x => x.Priority))
                    .ThenInclude(s => s.Children
                        .Where(x => x.IsMenu)
                        .OrderBy(x => x.Priority))
                    .AsQueryable();
            }
            else
            {
                query = query
                    .Include(s => s.Children
                        .Where(x => x.IsMenu && x.Roles.Any(a => a.Id == _currentUser.RoleId))
                        .OrderBy(x => x.Priority))
                    .ThenInclude(s => s.Children
                        .Where(x => x.IsMenu && x.Roles.Any(a => a.Id == _currentUser.RoleId))
                        .OrderBy(x => x.Priority))
                    .AsQueryable();
                query = query.Where(s => s.Roles.Any(a => a.Id == _currentUser.RoleId));
            }

            return new BaseResultDto<List<PermissionMenuDto>>(true, mapper.Map<List<PermissionMenuDto>>(query));
        }
    }
}
