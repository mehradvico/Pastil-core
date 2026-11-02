using Application.Common.Dto.Result;
using Application.Services.Accounting.PetMicrochipSrv;
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
    public interface IProviderUserService
    {
        // ---------------------------------------------- مالک ارائه‌دهنده (ownerUserId همیشه کاربر واقعیِ توکن است، نه هویت جانشین)
        Task<BaseResultDto<List<ProviderPermissionScopeVDto>>> CatalogAsync(long ownerUserId);
        Task<BaseResultDto<List<ProviderUserRoleVDto>>> ListRolesAsync(long ownerUserId);
        Task<BaseResultDto<ProviderUserRoleVDto>> SaveRoleAsync(long ownerUserId, ProviderUserRoleInputDto dto);
        Task<BaseResultDto> DeleteRoleAsync(long ownerUserId, long roleId);
        Task<BaseResultDto<List<ProviderUserVDto>>> ListUsersAsync(long ownerUserId);
        Task<BaseResultDto<ProviderUserVDto>> InviteAsync(long ownerUserId, ProviderUserInviteDto dto);
        Task<BaseResultDto> ChangeRoleAsync(long ownerUserId, ProviderUserRoleChangeDto dto);
        Task<BaseResultDto> SetActiveAsync(long ownerUserId, long providerUserId, bool active);
        Task<BaseResultDto> RemoveAsync(long ownerUserId, long providerUserId);

        // ---------------------------------------------- خود کارمند
        Task<List<ProviderUserInviteVDto>> ListInvitesAsync(long userId);
        Task<BaseResultDto> RespondInviteAsync(long providerUserId, long userId, bool accept);
        Task<List<ProviderMembershipVDto>> ListMembershipsAsync(long userId);
        Task<BaseResultDto> LeaveAsync(long providerUserId, long userId);

        // ---------------------------------------------- هر درخواست پنل با هدر X-Provider-Id
        /// <summary>
        /// کاربر الان کارمند فعال و پذیرفته‌شده‌ی این ارائه‌دهنده است؟ (ارائه‌دهنده فعال و تأییدشده، مالک قفل/حذف نشده، نقش حذف نشده).
        /// هر بار از دیتابیس خوانده می‌شود (بدون کش) تا غیرفعال‌کردن/حذف/تغییر نقش فوراً اثر کند.
        /// </summary>
        Task<ProviderActingAccess> ResolveActingAsync(long userId, long providerId);
    }

    /// <summary>
    /// کارمندان پنل ارائه‌دهنده و نقش‌هایشان. قاعده‌های امنیتی:
    /// - فقط مالک (Provider.UserId) نقش می‌سازد، دسترسی می‌دهد و کارمند دعوت/حذف می‌کند؛ هیچ کارمندی حتی با همه‌ی دسترسی‌ها نمی‌تواند.
    /// - دسترسی‌ها فقط کلیدهای <see cref="ProviderPermissionCatalog"/> و فقط برای نمایندگی‌هایی که ارائه‌دهنده واقعاً دارد.
    /// - هر شناسه (نقش/کارمند) قبل از هر تغییر با ProviderId همان مالک چک می‌شود؛ شناسه‌ی ارائه‌دهنده‌ی دیگر = «پیدا نشد».
    /// </summary>
    public class ProviderUserService : IProviderUserService
    {
        public const int MaxRolesPerProvider = 50;
        public const int MaxUsersPerProvider = 200;
        private const int MaxNameLength = 100;
        private const int MaxDescriptionLength = 500;

        private readonly IDataBaseContext _context;
        private readonly ILogger<ProviderUserService> _logger;

        public ProviderUserService(IDataBaseContext context, ILogger<ProviderUserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ------------------------------------------------------------------ کمکی‌ها

        private Task<Provider> OwnedProviderAsync(long ownerUserId) =>
            _context.Providers.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == ownerUserId && !p.Deleted);

        /// <summary>نمایندگی‌هایی که ارائه‌دهنده دارد: نقش تأییدشده‌ی ادمین یا دست‌کم یک مرکز ثبت‌شده از همان نوع.</summary>
        public async Task<HashSet<string>> ScopesOfProviderAsync(long providerId, long ownerUserId)
        {
            var scopes = new HashSet<string>(StringComparer.Ordinal);
            var roleLabels = await _context.ProviderRoleAssignments.AsNoTracking()
                .Where(a => a.ProviderId == providerId && !a.Deleted && a.Approved && !a.ProviderRole.Deleted && a.ProviderRole.Active)
                .Select(a => a.ProviderRole.Label)
                .ToListAsync();
            foreach (var label in roleLabels)
                scopes.Add(label);

            if (await _context.Stores.AsNoTracking().AnyAsync(s => s.ProviderId == providerId && !s.Deleted))
                scopes.Add(ProviderRoleLabels.Store);
            if (await _context.Clinics.AsNoTracking().AnyAsync(c => (c.ProviderId == providerId || c.OwnerId == ownerUserId) && !c.Deleted))
                scopes.Add(ProviderRoleLabels.Clinic);
            if (await _context.Groomers.AsNoTracking().AnyAsync(c => (c.ProviderId == providerId || c.OwnerId == ownerUserId) && !c.Deleted))
                scopes.Add(ProviderRoleLabels.Groomer);
            if (await _context.Coaches.AsNoTracking().AnyAsync(c => (c.ProviderId == providerId || c.OwnerId == ownerUserId) && !c.Deleted))
                scopes.Add(ProviderRoleLabels.Coach);
            if (await _context.Pansions.AsNoTracking().AnyAsync(p => !p.Deleted && (p.Companion.ProviderId == providerId || p.Companion.OwnerId == ownerUserId)))
                scopes.Add(ProviderRoleLabels.Pansion);
            if (await _context.Schools.AsNoTracking().AnyAsync(s => !s.Deleted && (s.ProviderId == providerId || (s.Companion != null && (s.Companion.ProviderId == providerId || s.Companion.OwnerId == ownerUserId)))))
                scopes.Add(ProviderRoleLabels.School);

            // فقط کلیدهای شناخته‌شده‌ی کاتالوگ (Label دلخواهی که ادمین ساخته، دسترسی‌ای نمی‌سازد)
            scopes.IntersectWith(ProviderPermissionCatalog.Scopes.Select(s => s.Key));
            return scopes;
        }

        private static string ScopeName(string scope) => scope switch
        {
            ProviderRoleLabels.Store => Resource.Notification.ProviderPermissionScopeStore,
            ProviderRoleLabels.Clinic => Resource.Notification.ProviderPermissionScopeClinic,
            ProviderRoleLabels.Groomer => Resource.Notification.ProviderPermissionScopeGroomer,
            ProviderRoleLabels.Coach => Resource.Notification.ProviderPermissionScopeCoach,
            ProviderRoleLabels.Pansion => Resource.Notification.ProviderPermissionScopePansion,
            ProviderRoleLabels.School => Resource.Notification.ProviderPermissionScopeSchool,
            _ => scope,
        };

        private static string SectionName(string section) => section switch
        {
            ProviderPermissionCatalog.Sections.Profile => Resource.Notification.ProviderPermissionSectionProfile,
            ProviderPermissionCatalog.Sections.Service => Resource.Notification.ProviderPermissionSectionService,
            ProviderPermissionCatalog.Sections.Reserve => Resource.Notification.ProviderPermissionSectionReserve,
            ProviderPermissionCatalog.Sections.Consultation => Resource.Notification.ProviderPermissionSectionConsultation,
            ProviderPermissionCatalog.Sections.Review => Resource.Notification.ProviderPermissionSectionReview,
            ProviderPermissionCatalog.Sections.Staff => Resource.Notification.ProviderPermissionSectionStaff,
            ProviderPermissionCatalog.Sections.Finance => Resource.Notification.ProviderPermissionSectionFinance,
            ProviderPermissionCatalog.Sections.Portfolio => Resource.Notification.ProviderPermissionSectionPortfolio,
            ProviderPermissionCatalog.Sections.Course => Resource.Notification.ProviderPermissionSectionCourse,
            ProviderPermissionCatalog.Sections.Product => Resource.Notification.ProviderPermissionSectionProduct,
            ProviderPermissionCatalog.Sections.Order => Resource.Notification.ProviderPermissionSectionOrder,
            ProviderPermissionCatalog.Sections.Discount => Resource.Notification.ProviderPermissionSectionDiscount,
            _ => section,
        };

        private static string LevelName(string level) =>
            level == ProviderPermissionCatalog.Manage ? Resource.Notification.ProviderPermissionLevelManage : Resource.Notification.ProviderPermissionLevelView;

        private static string ProviderDisplayName(string firstName, string lastName) =>
            string.Join(" ", new[] { firstName, lastName }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();

        // ------------------------------------------------------------------ کاتالوگ و نقش‌ها (مالک)

        public async Task<BaseResultDto<List<ProviderPermissionScopeVDto>>> CatalogAsync(long ownerUserId)
        {
            var provider = await OwnedProviderAsync(ownerUserId);
            if (provider == null)
                return new BaseResultDto<List<ProviderPermissionScopeVDto>>(false, Resource.Notification.ProviderOwnerOnlyAction, null);

            var scopes = await ScopesOfProviderAsync(provider.Id, ownerUserId);
            var tree = ProviderPermissionCatalog.Scopes
                .Where(scope => scopes.Contains(scope.Key))
                .Select(scope => new ProviderPermissionScopeVDto
                {
                    Key = scope.Key,
                    Name = ScopeName(scope.Key),
                    Sections = scope.Sections.Select(section => new ProviderPermissionSectionVDto
                    {
                        Key = section.Key,
                        Name = SectionName(section.Key),
                        Permissions = (section.HasManage
                                ? new[] { ProviderPermissionCatalog.View, ProviderPermissionCatalog.Manage }
                                : new[] { ProviderPermissionCatalog.View })
                            .Select(level => new ProviderPermissionVDto
                            {
                                Key = ProviderPermissionCatalog.Key(scope.Key, section.Key, level),
                                Level = level,
                                Name = LevelName(level),
                            }).ToList(),
                    }).ToList(),
                }).ToList();
            return new BaseResultDto<List<ProviderPermissionScopeVDto>>(true, tree);
        }

        public async Task<BaseResultDto<List<ProviderUserRoleVDto>>> ListRolesAsync(long ownerUserId)
        {
            var provider = await OwnedProviderAsync(ownerUserId);
            if (provider == null)
                return new BaseResultDto<List<ProviderUserRoleVDto>>(false, Resource.Notification.ProviderOwnerOnlyAction, null);

            var roles = await _context.ProviderUserRoles.AsNoTracking()
                .Where(r => r.ProviderId == provider.Id && !r.Deleted)
                .OrderBy(r => r.Id)
                .Select(r => new ProviderUserRoleVDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    CreateDate = r.CreateDate,
                    PermissionKeys = r.Permissions.OrderBy(p => p.PermissionKey).Select(p => p.PermissionKey).ToList(),
                    UserCount = r.Users.Count(u => !u.Deleted),
                }).ToListAsync();
            // کلیدی که از کاتالوگ حذف شده دیگر نمایش داده نمی‌شود (در زمان اجرا هم هیچ‌وقت اعتبار ندارد)
            roles.ForEach(r => r.PermissionKeys = r.PermissionKeys.Where(ProviderPermissionCatalog.IsKnown).ToList());
            return new BaseResultDto<List<ProviderUserRoleVDto>>(true, roles);
        }

        public async Task<BaseResultDto<ProviderUserRoleVDto>> SaveRoleAsync(long ownerUserId, ProviderUserRoleInputDto dto)
        {
            try
            {
                var provider = await OwnedProviderAsync(ownerUserId);
                if (provider == null)
                    return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderOwnerOnlyAction, null);
                if (dto == null)
                    return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.InvalidData, null);

                var name = dto.Name?.Trim();
                if (string.IsNullOrEmpty(name) || name.Length > MaxNameLength)
                    return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderUserRoleNameRequired, null);
                var description = dto.Description?.Trim();
                if (description?.Length > MaxDescriptionLength)
                    return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.InvalidData, null);

                var scopes = await ScopesOfProviderAsync(provider.Id, ownerUserId);
                var keys = ProviderPermissionCatalog.Normalize(dto.PermissionKeys, scopes);
                if (keys == null)
                    return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderUserRolePermissionInvalid, null);

                if (await _context.ProviderUserRoles.AnyAsync(r => r.ProviderId == provider.Id && !r.Deleted && r.Name == name && r.Id != dto.Id))
                    return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderUserRoleNameDuplicate, null);

                ProviderUserRole role;
                if (dto.Id > 0)
                {
                    role = await _context.ProviderUserRoles.AsTracking()
                        .FirstOrDefaultAsync(r => r.Id == dto.Id && r.ProviderId == provider.Id && !r.Deleted);
                    if (role == null)
                        return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderUserRoleNotFound, null);
                }
                else
                {
                    if (await _context.ProviderUserRoles.CountAsync(r => r.ProviderId == provider.Id && !r.Deleted) >= MaxRolesPerProvider)
                        return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderUserLimitReached, null);
                    role = new ProviderUserRole { ProviderId = provider.Id, CreateDate = DateTime.Now };
                    await _context.ProviderUserRoles.AddAsync(role);
                }

                role.Name = name;
                role.Description = string.IsNullOrEmpty(description) ? null : description;
                await _context.SaveChangesAsync();

                // جایگزینی کامل دسترسی‌ها (کم‌کردن دسترسی در همان درخواست بعدیِ کارمند اثر می‌کند؛ کشی در کار نیست)
                await _context.ProviderUserRolePermissions.Where(p => p.ProviderUserRoleId == role.Id).ExecuteDeleteAsync();
                if (keys.Count > 0)
                {
                    await _context.ProviderUserRolePermissions.AddRangeAsync(keys.Select(key => new ProviderUserRolePermission { ProviderUserRoleId = role.Id, PermissionKey = key }));
                    await _context.SaveChangesAsync();
                }

                var saved = (await ListRolesAsync(ownerUserId)).Data?.FirstOrDefault(r => r.Id == role.Id);
                return new BaseResultDto<ProviderUserRoleVDto>(true, saved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saving provider user role failed.");
                return new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        public async Task<BaseResultDto> DeleteRoleAsync(long ownerUserId, long roleId)
        {
            var provider = await OwnedProviderAsync(ownerUserId);
            if (provider == null)
                return new BaseResultDto(false, Resource.Notification.ProviderOwnerOnlyAction);
            var role = await _context.ProviderUserRoles.AsTracking()
                .FirstOrDefaultAsync(r => r.Id == roleId && r.ProviderId == provider.Id && !r.Deleted);
            if (role == null)
                return new BaseResultDto(false, Resource.Notification.ProviderUserRoleNotFound);
            if (await _context.ProviderUsers.AnyAsync(u => u.ProviderUserRoleId == roleId && !u.Deleted))
                return new BaseResultDto(false, Resource.Notification.ProviderUserRoleInUse);

            role.Deleted = true;
            await _context.SaveChangesAsync();
            await _context.ProviderUserRolePermissions.Where(p => p.ProviderUserRoleId == roleId).ExecuteDeleteAsync();
            return new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ کارمندان (مالک)

        public async Task<BaseResultDto<List<ProviderUserVDto>>> ListUsersAsync(long ownerUserId)
        {
            var provider = await OwnedProviderAsync(ownerUserId);
            if (provider == null)
                return new BaseResultDto<List<ProviderUserVDto>>(false, Resource.Notification.ProviderOwnerOnlyAction, null);

            var list = await _context.ProviderUsers.AsNoTracking()
                .Where(u => u.ProviderId == provider.Id && !u.Deleted)
                .OrderBy(u => u.Id)
                .Select(u => new ProviderUserVDto
                {
                    Id = u.Id,
                    UserId = u.UserId,
                    FirstName = u.User.FirstName,
                    LastName = u.User.LastName,
                    Mobile = u.User.Mobile,
                    ProviderUserRoleId = u.ProviderUserRoleId,
                    RoleName = u.ProviderUserRole.Name,
                    UserAccept = u.UserAccept,
                    InviteDate = u.InviteDate,
                    Active = u.Active,
                }).ToListAsync();
            return new BaseResultDto<List<ProviderUserVDto>>(true, list);
        }

        private Task<bool> RoleBelongsAsync(long providerId, long roleId) =>
            _context.ProviderUserRoles.AsNoTracking().AnyAsync(r => r.Id == roleId && r.ProviderId == providerId && !r.Deleted);

        public async Task<BaseResultDto<ProviderUserVDto>> InviteAsync(long ownerUserId, ProviderUserInviteDto dto)
        {
            try
            {
                var provider = await OwnedProviderAsync(ownerUserId);
                if (provider == null)
                    return new BaseResultDto<ProviderUserVDto>(false, Resource.Notification.ProviderOwnerOnlyAction, null);
                if (dto == null || !await RoleBelongsAsync(provider.Id, dto.ProviderUserRoleId))
                    return new BaseResultDto<ProviderUserVDto>(false, Resource.Notification.ProviderUserRoleNotFound, null);

                var mobile = PetMicrochipRules.NormalizeMobile(dto.Mobile);
                if (mobile == null)
                    return new BaseResultDto<ProviderUserVDto>(false, Resource.Notification.InvalidData, null);
                var user = await _context.Users.AsNoTracking()
                    .Where(u => !u.Deleted && !u.Locked && u.Mobile == mobile)
                    .Select(u => new { u.Id, u.FirstName, u.LastName, u.Mobile })
                    .FirstOrDefaultAsync();
                if (user == null)
                    return new BaseResultDto<ProviderUserVDto>(false, Resource.Notification.NothingFound, null);
                if (user.Id == provider.UserId)
                    return new BaseResultDto<ProviderUserVDto>(false, Resource.Notification.ProviderUserCannotInviteSelf, null);

                // دعوتِ ردشده را می‌شود دوباره فرستاد؛ دعوت منتظر یا کارمند فعلی تکراری است.
                var existing = await _context.ProviderUsers.AsTracking()
                    .FirstOrDefaultAsync(u => u.ProviderId == provider.Id && u.UserId == user.Id && !u.Deleted);
                if (existing != null && existing.UserAccept != false)
                    return new BaseResultDto<ProviderUserVDto>(false, Resource.Notification.ProviderUserAlreadyExists, null);
                if (existing == null && await _context.ProviderUsers.CountAsync(u => u.ProviderId == provider.Id && !u.Deleted) >= MaxUsersPerProvider)
                    return new BaseResultDto<ProviderUserVDto>(false, Resource.Notification.ProviderUserLimitReached, null);

                var now = DateTime.Now;
                var staff = existing ?? new ProviderUser { ProviderId = provider.Id, UserId = user.Id, CreateDate = now };
                staff.ProviderUserRoleId = dto.ProviderUserRoleId;
                staff.Active = true;
                staff.UserAccept = null;
                staff.InviteDate = now;
                if (existing == null)
                    await _context.ProviderUsers.AddAsync(staff);
                await _context.SaveChangesAsync();

                var roleName = await _context.ProviderUserRoles.AsNoTracking().Where(r => r.Id == staff.ProviderUserRoleId).Select(r => r.Name).FirstAsync();
                return new BaseResultDto<ProviderUserVDto>(true, new ProviderUserVDto
                {
                    Id = staff.Id, UserId = user.Id, FirstName = user.FirstName, LastName = user.LastName, Mobile = user.Mobile,
                    ProviderUserRoleId = staff.ProviderUserRoleId, RoleName = roleName, UserAccept = null, InviteDate = now, Active = true,
                });
            }
            catch (DbUpdateException ex)
            {
                // دو دعوت هم‌زمان برای یک نفر: ایندکس یکتا جلوی ردیف دوم را می‌گیرد
                _logger.LogWarning(ex, "Concurrent provider user invite rejected.");
                return new BaseResultDto<ProviderUserVDto>(false, Resource.Notification.ProviderUserAlreadyExists, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Inviting provider user failed.");
                return new BaseResultDto<ProviderUserVDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        public async Task<BaseResultDto> ChangeRoleAsync(long ownerUserId, ProviderUserRoleChangeDto dto)
        {
            var provider = await OwnedProviderAsync(ownerUserId);
            if (provider == null)
                return new BaseResultDto(false, Resource.Notification.ProviderOwnerOnlyAction);
            if (dto == null || !await RoleBelongsAsync(provider.Id, dto.ProviderUserRoleId))
                return new BaseResultDto(false, Resource.Notification.ProviderUserRoleNotFound);
            var updated = await _context.ProviderUsers
                .Where(u => u.Id == dto.Id && u.ProviderId == provider.Id && !u.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.ProviderUserRoleId, dto.ProviderUserRoleId));
            return updated == 0 ? new BaseResultDto(false, Resource.Notification.ProviderUserNotFound) : new BaseResultDto(true);
        }

        public async Task<BaseResultDto> SetActiveAsync(long ownerUserId, long providerUserId, bool active)
        {
            var provider = await OwnedProviderAsync(ownerUserId);
            if (provider == null)
                return new BaseResultDto(false, Resource.Notification.ProviderOwnerOnlyAction);
            var updated = await _context.ProviderUsers
                .Where(u => u.Id == providerUserId && u.ProviderId == provider.Id && !u.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Active, active));
            return updated == 0 ? new BaseResultDto(false, Resource.Notification.ProviderUserNotFound) : new BaseResultDto(true);
        }

        public async Task<BaseResultDto> RemoveAsync(long ownerUserId, long providerUserId)
        {
            var provider = await OwnedProviderAsync(ownerUserId);
            if (provider == null)
                return new BaseResultDto(false, Resource.Notification.ProviderOwnerOnlyAction);
            var updated = await _context.ProviderUsers
                .Where(u => u.Id == providerUserId && u.ProviderId == provider.Id && !u.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Deleted, true).SetProperty(u => u.Active, false));
            return updated == 0 ? new BaseResultDto(false, Resource.Notification.ProviderUserNotFound) : new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ خود کارمند

        public async Task<List<ProviderUserInviteVDto>> ListInvitesAsync(long userId)
        {
            var rows = await _context.ProviderUsers.AsNoTracking()
                .Where(u => u.UserId == userId && !u.Deleted && u.UserAccept == null && !u.Provider.Deleted && u.Provider.Active)
                .OrderByDescending(u => u.Id)
                .Select(u => new
                {
                    u.Id,
                    u.ProviderId,
                    OwnerFirstName = u.Provider.User.FirstName,
                    OwnerLastName = u.Provider.User.LastName,
                    RoleName = u.ProviderUserRole.Name,
                    u.InviteDate,
                }).ToListAsync();
            return rows.Select(r => new ProviderUserInviteVDto
            {
                Id = r.Id,
                ProviderId = r.ProviderId,
                ProviderName = ProviderDisplayName(r.OwnerFirstName, r.OwnerLastName),
                RoleName = r.RoleName,
                InviteDate = r.InviteDate,
            }).ToList();
        }

        public async Task<BaseResultDto> RespondInviteAsync(long providerUserId, long userId, bool accept)
        {
            var staff = await _context.ProviderUsers.AsTracking()
                .FirstOrDefaultAsync(u => u.Id == providerUserId && u.UserId == userId && !u.Deleted);
            if (staff == null)
                return new BaseResultDto(false, Resource.Notification.ProviderUserInviteNotFound);
            if (staff.UserAccept != null)
                return new BaseResultDto(false, Resource.Notification.ProviderUserInviteAnswered);

            staff.UserAccept = accept;
            if (!accept)
                staff.Active = false;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task<List<ProviderMembershipVDto>> ListMembershipsAsync(long userId)
        {
            var rows = await ActiveMembershipQuery(userId)
                .OrderBy(u => u.Id)
                .Select(u => new
                {
                    u.Id,
                    u.ProviderId,
                    OwnerFirstName = u.Provider.User.FirstName,
                    OwnerLastName = u.Provider.User.LastName,
                    u.ProviderUserRoleId,
                    RoleName = u.ProviderUserRole.Name,
                    Keys = u.ProviderUserRole.Permissions.Select(p => p.PermissionKey).ToList(),
                }).ToListAsync();

            return rows.Select(r =>
            {
                var keys = r.Keys.Where(ProviderPermissionCatalog.IsKnown).Distinct().OrderBy(k => k, StringComparer.Ordinal).ToList();
                return new ProviderMembershipVDto
                {
                    ProviderUserId = r.Id,
                    ProviderId = r.ProviderId,
                    ProviderName = ProviderDisplayName(r.OwnerFirstName, r.OwnerLastName),
                    ProviderUserRoleId = r.ProviderUserRoleId,
                    RoleName = r.RoleName,
                    Permissions = keys,
                    Scopes = keys.Select(ProviderPermissionCatalog.ScopeOf).Distinct().ToList(),
                };
            }).ToList();
        }

        public async Task<BaseResultDto> LeaveAsync(long providerUserId, long userId)
        {
            var updated = await _context.ProviderUsers
                .Where(u => u.Id == providerUserId && u.UserId == userId && !u.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Deleted, true).SetProperty(u => u.Active, false));
            return updated == 0 ? new BaseResultDto(false, Resource.Notification.ProviderUserNotFound) : new BaseResultDto(true);
        }

        // ------------------------------------------------------------------ اعتبارسنجی هر درخواست

        /// <summary>عضویت معتبر: کارمند فعال و پذیرفته، نقش زنده، ارائه‌دهنده فعال/تأییدشده، مالک قفل/حذف نشده، و کارمند خودِ مالک نیست.</summary>
        private IQueryable<ProviderUser> ActiveMembershipQuery(long userId) =>
            _context.ProviderUsers.AsNoTracking()
                .Where(u => u.UserId == userId && !u.Deleted && u.Active && u.UserAccept == true
                    && !u.ProviderUserRole.Deleted && u.ProviderUserRole.ProviderId == u.ProviderId
                    && !u.Provider.Deleted && u.Provider.Active && u.Provider.Approved
                    && u.Provider.UserId != userId
                    && !u.Provider.User.Deleted && !u.Provider.User.Locked);

        public async Task<ProviderActingAccess> ResolveActingAsync(long userId, long providerId)
        {
            if (userId <= 0 || providerId <= 0)
                return null;

            var row = await ActiveMembershipQuery(userId)
                .Where(u => u.ProviderId == providerId)
                .Select(u => new
                {
                    u.Id,
                    u.ProviderId,
                    OwnerUserId = u.Provider.UserId,
                    u.ProviderUserRoleId,
                    Keys = u.ProviderUserRole.Permissions.Select(p => p.PermissionKey).ToList(),
                }).FirstOrDefaultAsync();
            if (row == null)
                return null;

            return new ProviderActingAccess
            {
                ProviderId = row.ProviderId,
                ProviderUserId = row.Id,
                OwnerUserId = row.OwnerUserId,
                ProviderUserRoleId = row.ProviderUserRoleId,
                Permissions = new HashSet<string>(row.Keys.Where(ProviderPermissionCatalog.IsKnown), StringComparer.Ordinal),
            };
        }
    }
}
