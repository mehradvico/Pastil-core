using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderUserSrv.Dto;
using Entities.Entities.ProviderField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.ProviderUserSrv
{
    public interface IProviderUserRoleTemplateService
    {
        List<ProviderPermissionScopeVDto> Catalog();
        Task<BaseResultDto<List<ProviderUserRoleVDto>>> ListAsync(string scope);
        Task<BaseResultDto<ProviderUserRoleVDto>> SaveAsync(ProviderUserRoleTemplateInputDto dto);
        Task<BaseResultDto> DeleteAsync(long id);
    }

    /// <summary>
    /// نقش‌های آماده‌ی کارمندان ارائه‌دهنده (۲۰۲۶-۱۱-۰۷) که ادمین اصلی برای هر نمایندگی تعریف می‌کند (مثلاً زیر «کلینیک»: مسئول کلینیک، پذیرش، حسابدار).
    /// ردیف ProviderUserRoles با ProviderId خالی و Scope پر. به هر ارائه‌دهنده فقط نقش‌های نمایندگی‌هایی که دارد نمایش داده می‌شود
    /// و مالک فقط انتسابشان می‌دهد (ویرایش/حذف فقط از همین‌جا). دسترسی‌ها فقط کلیدهای همان نمایندگی.
    /// </summary>
    public class ProviderUserRoleTemplateService : IProviderUserRoleTemplateService
    {
        private const int MaxNameLength = 100;
        private const int MaxDescriptionLength = 500;
        private const int MaxTemplatesPerScope = 50;

        private readonly IDataBaseContext _context;
        private readonly ILogger<ProviderUserRoleTemplateService> _logger;

        public ProviderUserRoleTemplateService(IDataBaseContext context, ILogger<ProviderUserRoleTemplateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private static bool IsKnownScope(string scope) =>
            scope != null && ProviderPermissionCatalog.Scopes.Any(s => s.Key == scope);

        public List<ProviderPermissionScopeVDto> Catalog() => ProviderUserService.BuildCatalogTree(null);

        public async Task<BaseResultDto<List<ProviderUserRoleVDto>>> ListAsync(string scope)
        {
            scope = scope?.Trim();
            if (!string.IsNullOrEmpty(scope) && !IsKnownScope(scope))
                return new BaseResultDto<List<ProviderUserRoleVDto>>(false, Resource.Notification.InvalidData, null);

            var query = _context.ProviderUserRoles.AsNoTracking().Where(r => r.ProviderId == null && !r.Deleted);
            if (!string.IsNullOrEmpty(scope))
                query = query.Where(r => r.Scope == scope);

            var list = await query
                .OrderBy(r => r.Scope).ThenBy(r => r.Priority).ThenBy(r => r.Id)
                .Select(r => new ProviderUserRoleVDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    CreateDate = r.CreateDate,
                    IsTemplate = true,
                    Scope = r.Scope,
                    Priority = r.Priority,
                    PermissionKeys = r.Permissions.OrderBy(p => p.PermissionKey).Select(p => p.PermissionKey).ToList(),
                    UserCount = r.Users.Count(u => !u.Deleted),
                }).ToListAsync();
            list.ForEach(r =>
            {
                r.PermissionKeys = r.PermissionKeys.Where(k => ProviderPermissionCatalog.ScopeOf(k) == r.Scope).ToList();
                r.ScopeName = ProviderUserService.ScopeDisplayName(r.Scope);
            });
            return new BaseResultDto<List<ProviderUserRoleVDto>>(true, list);
        }

        public async Task<BaseResultDto<ProviderUserRoleVDto>> SaveAsync(ProviderUserRoleTemplateInputDto dto)
        {
            try
            {
                if (dto == null)
                    return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.InvalidData, null);

                var name = dto.Name?.Trim();
                if (string.IsNullOrEmpty(name) || name.Length > MaxNameLength)
                    return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderUserRoleNameRequired, null);
                var description = dto.Description?.Trim();
                if (description?.Length > MaxDescriptionLength)
                    return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.InvalidData, null);

                ProviderUserRole role = null;
                if (dto.Id > 0)
                {
                    role = await _context.ProviderUserRoles.AsTracking()
                        .FirstOrDefaultAsync(r => r.Id == dto.Id && r.ProviderId == null && !r.Deleted);
                    if (role == null)
                        return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderUserRoleNotFound, null);
                }

                // نمایندگیِ نقش موجود عوض نمی‌شود (کارمندانی که این نقش را دارند ناگهان به نمایندگی دیگری دسترسی نگیرند)
                var scope = role?.Scope ?? dto.Scope?.Trim();
                if (!IsKnownScope(scope))
                    return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.InvalidData, null);

                var keys = ProviderPermissionCatalog.Normalize(dto.PermissionKeys, new[] { scope });
                if (keys == null)
                    return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderUserRolePermissionInvalid, null);

                if (await _context.ProviderUserRoles.AnyAsync(r => r.ProviderId == null && !r.Deleted && r.Scope == scope && r.Name == name && r.Id != dto.Id))
                    return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderUserRoleNameDuplicate, null);

                if (role == null)
                {
                    if (await _context.ProviderUserRoles.CountAsync(r => r.ProviderId == null && !r.Deleted && r.Scope == scope) >= MaxTemplatesPerScope)
                        return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderUserLimitReached, null);
                    role = new ProviderUserRole { ProviderId = null, Scope = scope, CreateDate = DateTime.Now };
                    await _context.ProviderUserRoles.AddAsync(role);
                }

                role.Name = name;
                role.Description = string.IsNullOrEmpty(description) ? null : description;
                role.Priority = dto.Priority;
                await _context.SaveChangesAsync();

                // جایگزینی کامل دسترسی‌ها؛ از درخواست بعدیِ همه‌ی کارمندانِ این نقش در همه‌ی ارائه‌دهنده‌ها اثر می‌کند
                await _context.ProviderUserRolePermissions.Where(p => p.ProviderUserRoleId == role.Id).ExecuteDeleteAsync();
                if (keys.Count > 0)
                {
                    await _context.ProviderUserRolePermissions.AddRangeAsync(keys.Select(key => new ProviderUserRolePermission { ProviderUserRoleId = role.Id, PermissionKey = key }));
                    await _context.SaveChangesAsync();
                }

                var saved = (await ListAsync(scope)).Data?.FirstOrDefault(r => r.Id == role.Id);
                return new BaseResultDto<ProviderUserRoleVDto>(true, saved);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Saving provider user role template rejected (duplicate name?).");
                return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderUserRoleNameDuplicate, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saving provider user role template failed.");
                return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        public async Task<BaseResultDto> DeleteAsync(long id)
        {
            var role = await _context.ProviderUserRoles.AsTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.ProviderId == null && !r.Deleted);
            if (role == null)
                return new BaseResultDto(false, Resource.Notification.ProviderUserRoleNotFound);
            if (await _context.ProviderUsers.AnyAsync(u => u.ProviderUserRoleId == id && !u.Deleted))
                return new BaseResultDto(false, Resource.Notification.ProviderUserRoleInUse);

            role.Deleted = true;
            await _context.SaveChangesAsync();
            await _context.ProviderUserRolePermissions.Where(p => p.ProviderUserRoleId == id).ExecuteDeleteAsync();
            return new BaseResultDto(true);
        }
    }
}
