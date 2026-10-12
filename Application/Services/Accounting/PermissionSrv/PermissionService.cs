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
            var isAdmin = _currentUser.RoleEnum == RoleEnum.Admin.ToString();
            var roleId = _currentUser.RoleId;

            var all = _context.Permissions
                .Where(s => !s.Deleted)
                .Select(s => new MenuNode
                {
                    Id = s.Id,
                    Name = s.Name,
                    Label = s.Label,
                    Area = s.Area,
                    Controller = s.Controller,
                    Action = s.Action,
                    IsMenu = s.IsMenu,
                    Priority = s.Priority,
                    ParentId = s.ParentId,
                    Granted = isAdmin || s.Roles.Any(a => a.Id == roleId)
                })
                .ToList();

            var children = all
                .Where(s => s.ParentId.HasValue)
                .GroupBy(s => s.ParentId.Value)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Priority).ThenBy(s => s.Id).ToList());

            List<MenuNode> ChildrenOf(long id) => children.GetValueOrDefault(id) ?? new List<MenuNode>();

            // آیتم منوی یک کنترلر + زیرمنوهای همان کنترلر (actionهای IsMenu)، مثل رفتار قبلی.
            PermissionMenuDto ToDto(MenuNode node, bool withChildren) => new PermissionMenuDto
            {
                Name = node.Name,
                Label = node.Label,
                Area = node.Area,
                Controller = node.Controller,
                Action = node.Action,
                IsMenu = node.IsMenu,
                Priority = node.Priority,
                ParentId = node.ParentId,
                Children = withChildren
                    ? ChildrenOf(node.Id)
                        .Where(c => c.IsMenu && c.Granted && string.Equals(c.Controller, node.Controller, System.StringComparison.OrdinalIgnoreCase))
                        .Select(c => ToDto(c, false))
                        .ToList()
                    : new List<PermissionMenuDto>()
            };

            // درخت دسترسی‌ها حالا چندسطحی است (گروه ← بخش ← زیربخش)؛ سایدبار اما فقط «گروه ← صفحه» است.
            // پس هر کنترلرِ منو‌دار در هر عمقی، به ترتیب درخت، مستقیم زیر گروه خودش نمایش داده می‌شود.
            void CollectMenuItems(MenuNode parent, List<PermissionMenuDto> target)
            {
                foreach (var child in ChildrenOf(parent.Id))
                {
                    var isOwnAction = parent.ParentId.HasValue &&
                        string.Equals(child.Controller, parent.Controller, System.StringComparison.OrdinalIgnoreCase);

                    if (child.IsMenu && child.Granted && !isOwnAction)
                        target.Add(ToDto(child, true));

                    CollectMenuItems(child, target);
                }
            }

            var menu = all
                .Where(s => !s.ParentId.HasValue && s.Granted)
                .OrderBy(s => s.Priority)
                .Select(group =>
                {
                    var items = new List<PermissionMenuDto>();
                    CollectMenuItems(group, items);
                    var dto = ToDto(group, false);
                    dto.Children = items;
                    return dto;
                })
                .ToList();

            return new BaseResultDto<List<PermissionMenuDto>>(true, menu);
        }

        private sealed class MenuNode
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public string Label { get; set; }
            public string Area { get; set; }
            public string Controller { get; set; }
            public string Action { get; set; }
            public bool IsMenu { get; set; }
            public int Priority { get; set; }
            public long? ParentId { get; set; }
            public bool Granted { get; set; }
        }

        /// <summary>
        /// فهرست controller/actionهای ناحیه Admin که نقش کاربر جاری به آن‌ها دسترسی دارد (برای غیرفعال‌کردن دکمه‌های افزودن/ویرایش در پنل).
        /// ادمین کل (RoleEnum.Admin) همه‌چیز را دارد.
        /// </summary>
        public BaseResultDto MyAdminActions()
        {
            if (_currentUser.RoleEnum == RoleEnum.Admin.ToString())
                return new BaseResultDto<PermissionActionsDto>(true, new PermissionActionsDto { IsFullAccess = true, Items = new List<PermissionActionItemDto>() });

            var items = _context.Permissions
                .Where(s => !s.Deleted && s.Area != null && s.Area.ToLower() == "admin" && s.Controller != null && s.Action != null
                            && s.Roles.Any(a => a.Id == _currentUser.RoleId))
                .Select(s => new PermissionActionItemDto { Controller = s.Controller, Action = s.Action })
                .ToList();

            return new BaseResultDto<PermissionActionsDto>(true, new PermissionActionsDto { IsFullAccess = false, Items = items });
        }
    }
}
