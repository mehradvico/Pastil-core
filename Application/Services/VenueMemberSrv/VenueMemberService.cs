using Application.Common.Dto.Result;
using Application.Services.Filing.PictureSrv.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.VenueMemberSrv
{
    /// <summary>نقش عضو در کارت «اعضای مرکز»: ۱ مالک/مدیر مرکز، ۲ عضو تیم.</summary>
    public enum VenueMemberRoleEnum
    {
        Owner = 1,
        Member = 2,
    }

    public class VenueMemberVDto
    {
        /// <summary>شناسه‌ی کاربرِ عضو.</summary>
        public long Id { get; set; }
        public string FullName { get; set; }
        public PictureVDto Picture { get; set; }
        public int RoleId { get; set; }
        /// <summary>«مدیر مرکز» برای مالک؛ برای پرسنل نام خدماتی که به آن‌ها وصل است (یا «عضو تیم»).</summary>
        public string RoleTitle { get; set; }
        public int SortOrder { get; set; }
    }

    public interface IVenueMemberService
    {
        Task<BaseResultDto<List<VenueMemberVDto>>> ClinicMembersAsync(long clinicId, int? page, int? pageSize);
        Task<BaseResultDto<List<VenueMemberVDto>>> CoachMembersAsync(long coachId, int? page, int? pageSize);
    }

    /// <summary>
    /// لیست عمومی اعضای کلینیک/مربی: مالک + پرسنل پذیرفته‌شده، فعال و حذف‌نشده؛ فقط نام، عکس و عنوان نقش (بدون موبایل/ایمیل).
    /// کنترلر قبل از صدا زدن، عمومی بودن مرکز را با FindPublicAsync بررسی می‌کند. طراحی: backend/Docs/VENUE_MEMBERS_NEMAYANDE_APP_FA.md
    /// </summary>
    public class VenueMemberService : IVenueMemberService
    {
        public const int MaxPageSize = 50;
        private static readonly TimeSpan CacheFor = TimeSpan.FromSeconds(60);

        private readonly IDataBaseContext _context;
        private readonly IMemoryCache _cache;

        public VenueMemberService(IDataBaseContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public Task<BaseResultDto<List<VenueMemberVDto>>> ClinicMembersAsync(long clinicId, int? page, int? pageSize) =>
            CachedAsync("clinic", clinicId, page, pageSize, async () =>
            {
                var ownerId = await _context.Clinics.AsNoTracking().Where(c => c.Id == clinicId).Select(c => (long?)c.OwnerId).FirstOrDefaultAsync();
                var staff = await _context.ClinicUsers.AsNoTracking()
                    .Where(u => u.ClinicId == clinicId && u.UserAccept == true && u.Active && !u.Deleted)
                    .OrderBy(u => u.InviteDate).ThenBy(u => u.Id).Select(u => u.UserId).ToListAsync();
                var services = await _context.ClinicServiceUsers.AsNoTracking()
                    .Where(s => s.ClinicService.ClinicId == clinicId && s.Active && !s.Deleted && !s.ClinicService.Deleted)
                    .OrderBy(s => s.Id)
                    .Select(s => new ServiceLink(s.UserId, s.ClinicService.Assistance.Name)).ToListAsync();
                return await BuildAsync(ownerId, staff, services);
            });

        public Task<BaseResultDto<List<VenueMemberVDto>>> CoachMembersAsync(long coachId, int? page, int? pageSize) =>
            CachedAsync("coach", coachId, page, pageSize, async () =>
            {
                var ownerId = await _context.Coaches.AsNoTracking().Where(c => c.Id == coachId).Select(c => (long?)c.OwnerId).FirstOrDefaultAsync();
                var staff = await _context.CoachUsers.AsNoTracking()
                    .Where(u => u.CoachId == coachId && u.UserAccept == true && u.Active && !u.Deleted)
                    .OrderBy(u => u.InviteDate).ThenBy(u => u.Id).Select(u => u.UserId).ToListAsync();
                var services = await _context.CoachServiceUsers.AsNoTracking()
                    .Where(s => s.CoachService.CoachId == coachId && s.Active && !s.Deleted && !s.CoachService.Deleted)
                    .OrderBy(s => s.Id)
                    .Select(s => new ServiceLink(s.UserId, s.CoachService.Assistance.Name)).ToListAsync();
                return await BuildAsync(ownerId, staff, services);
            });

        private sealed record ServiceLink(long UserId, string Name);

        // کل لیست مرتب‌شده‌ی یک مرکز ۶۰ ثانیه cache می‌شود؛ صفحه‌بندی روی همان انجام می‌شود
        private async Task<BaseResultDto<List<VenueMemberVDto>>> CachedAsync(string kind, long venueId, int? page, int? pageSize, Func<Task<List<VenueMemberVDto>>> load)
        {
            var key = "venue-members:" + kind + ":" + venueId;
            if (!_cache.TryGetValue(key, out List<VenueMemberVDto> all) || all == null)
            {
                all = await load();
                _cache.Set(key, all, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheFor, Size = 1 });
            }
            var (skip, take) = VenueMemberRules.Page(page, pageSize);
            return new BaseResultDto<List<VenueMemberVDto>>(true, all.Skip(skip).Take(take).ToList());
        }

        private async Task<List<VenueMemberVDto>> BuildAsync(long? ownerId, List<long> staffIds, List<ServiceLink> services)
        {
            var ids = staffIds.ToList();
            if (ownerId.HasValue)
                ids.Insert(0, ownerId.Value);
            ids = ids.Distinct().ToList();
            if (ids.Count == 0)
                return new List<VenueMemberVDto>();

            var users = await _context.Users.AsNoTracking()
                .Where(u => ids.Contains(u.Id))
                .Select(u => new
                {
                    u.Id, u.FirstName, u.LastName,
                    Picture = u.Picture == null ? null : new PictureVDto
                    {
                        Id = u.Picture.Id, Url = u.Picture.Url,
                        GuidName = u.Picture.GuidName, Extension = u.Picture.Extension, OrginalName = u.Picture.OrginalName,
                    },
                })
                .ToDictionaryAsync(u => u.Id);

            var result = new List<VenueMemberVDto>();
            foreach (var id in ids)
            {
                // کاربرِ حذف/غیرفعال‌شده (فیلتر سراسری User) نمایش داده نمی‌شود
                if (!users.TryGetValue(id, out var user))
                    continue;
                var isOwner = ownerId == id;
                result.Add(new VenueMemberVDto
                {
                    Id = id,
                    FullName = VenueMemberRules.FullName(user.FirstName, user.LastName),
                    Picture = user.Picture,
                    RoleId = (int)(isOwner ? VenueMemberRoleEnum.Owner : VenueMemberRoleEnum.Member),
                    RoleTitle = VenueMemberRules.RoleTitle(isOwner, services.Where(s => s.UserId == id).Select(s => s.Name)),
                    SortOrder = result.Count + 1,
                });
            }
            return result;
        }
    }

    public static class VenueMemberRules
    {
        public const int MaxRoleServices = 2;

        public static (int Skip, int Take) Page(int? page, int? pageSize)
        {
            var size = pageSize is > 0 ? Math.Min(pageSize.Value, VenueMemberService.MaxPageSize) : VenueMemberService.MaxPageSize;
            var index = page is > 0 ? page.Value : 1;
            return ((index - 1) * size, size);
        }

        public static string FullName(string firstName, string lastName)
        {
            var name = ((firstName ?? string.Empty) + " " + (lastName ?? string.Empty)).Trim();
            return string.IsNullOrWhiteSpace(name) ? Resource.Lang.VenueMemberDefaultRole : name;
        }

        // مالک = «مدیر مرکز»؛ پرسنل = تا ۲ نام خدمت، وگرنه «عضو تیم»
        public static string RoleTitle(bool isOwner, IEnumerable<string> serviceNames)
        {
            if (isOwner)
                return Resource.Lang.VenueMemberOwnerRole;
            var names = (serviceNames ?? Enumerable.Empty<string>())
                .Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).Distinct().Take(MaxRoleServices).ToList();
            return names.Count == 0 ? Resource.Lang.VenueMemberDefaultRole : string.Join(Resource.Lang.VenueMemberRoleSeparator, names);
        }
    }
}
