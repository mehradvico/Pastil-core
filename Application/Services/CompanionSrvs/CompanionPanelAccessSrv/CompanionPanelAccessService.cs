using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Persistence.Interface;
using System;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionPanelAccessSrv
{
    public class CompanionPanelAccessVDto
    {
        /// <summary>true = کاربر می‌تواند وارد پنل/اپ نماینده شود (ارائه‌دهنده است، یا مرکزی دارد، یا عضو تیم فعال یک مرکز است).</summary>
        public bool CanEnter { get; set; }
        /// <summary>روی کاربر یک ارائه‌دهنده (Provider) ثبت شده است.</summary>
        public bool IsProvider { get; set; }
        /// <summary>مالک حداقل یک مرکز است (نمایندگی، کلینیک، مربی یا آرایشگاه؛ حتی اگر هنوز «در حال بررسی» باشد).</summary>
        public bool OwnsCenter { get; set; }
        /// <summary>عضو فعال و تأییدشده‌ی تیم حداقل یک مرکز است (دعوت‌نامه‌ی پذیرفته‌نشده حساب نمی‌شود).</summary>
        public bool IsStaff { get; set; }
    }

    public interface ICompanionPanelAccessService
    {
        Task<CompanionPanelAccessVDto> GetAsync(long userId);
        /// <summary>برای فیلتر ناحیه‌ی Companion: فقط نتیجه‌ی مثبت ۶۰ ثانیه کش می‌شود تا کاربر تازه‌تأییدشده معطل نماند.</summary>
        Task<bool> CanEnterAsync(long userId);
    }

    /// <summary>
    /// ورود به ناحیه‌ی Companion (پنل/اپ نماینده) فقط برای کسی است که ارائه‌دهنده است یا مرکزی دارد یا عضو تیم یک مرکز است.
    /// مشتری معمولی بدون هیچ‌کدام از این‌ها به هیچ endpoint ناحیه‌ی Companion دسترسی ندارد.
    /// </summary>
    public class CompanionPanelAccessService : ICompanionPanelAccessService
    {
        private static readonly TimeSpan PositiveCacheTtl = TimeSpan.FromSeconds(60);

        private readonly IDataBaseContext _context;
        private readonly IMemoryCache _cache;

        public CompanionPanelAccessService(IDataBaseContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<CompanionPanelAccessVDto> GetAsync(long userId)
        {
            var isProvider = await _context.Providers.AsNoTracking().AnyAsync(p => p.UserId == userId && !p.Deleted);

            var ownsCenter =
                await _context.Companions.AsNoTracking().AnyAsync(c => c.OwnerId == userId && !c.Deleted) ||
                await _context.Clinics.AsNoTracking().AnyAsync(c => c.OwnerId == userId && !c.Deleted) ||
                await _context.Coaches.AsNoTracking().AnyAsync(c => c.OwnerId == userId && !c.Deleted) ||
                await _context.Groomers.AsNoTracking().AnyAsync(c => c.OwnerId == userId && !c.Deleted);

            var isStaff =
                await _context.CompanionUsers.AsNoTracking().AnyAsync(u => u.UserId == userId && !u.Deleted && u.Active && u.UserAccept == true) ||
                await _context.ClinicUsers.AsNoTracking().AnyAsync(u => u.UserId == userId && !u.Deleted && u.Active && u.UserAccept == true) ||
                await _context.CoachUsers.AsNoTracking().AnyAsync(u => u.UserId == userId && !u.Deleted && u.Active && u.UserAccept == true) ||
                await _context.GroomerUsers.AsNoTracking().AnyAsync(u => u.UserId == userId && !u.Deleted && u.Active && u.UserAccept == true);

            return new CompanionPanelAccessVDto
            {
                IsProvider = isProvider,
                OwnsCenter = ownsCenter,
                IsStaff = isStaff,
                CanEnter = isProvider || ownsCenter || isStaff,
            };
        }

        public async Task<bool> CanEnterAsync(long userId)
        {
            var key = "companion-panel-access:" + userId;
            if (_cache.TryGetValue(key, out bool cached) && cached)
                return true;

            var access = await GetAsync(userId);
            if (access.CanEnter)
                _cache.Set(key, true, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = PositiveCacheTtl, Size = 1 }); // کش برنامه SizeLimit دارد
            return access.CanEnter;
        }
    }
}
