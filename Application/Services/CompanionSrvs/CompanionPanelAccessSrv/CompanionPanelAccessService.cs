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
        /// <summary>عضو فعال و تأییدشده‌ی تیم حداقل یک مرکز است (دعوت‌نامه‌ی پذیرفته‌نشده حساب نمی‌شود). پرسنل فقط پنل اپراتور و endpointهای شخصی را دارد.</summary>
        public bool IsStaff { get; set; }
        /// <summary>
        /// کارمند فعال پنل حداقل یک ارائه‌دهنده است (ProviderUser). این به‌تنهایی CanEnter نمی‌دهد: کارمند باید با هدر X-Provider-Id و فقط در
        /// محدوده‌ی دسترسی نقشش وارد شود؛ لیست ارائه‌دهنده‌ها و دسترسی‌ها از GET api/EndUser/ProviderMembership می‌آید.
        /// </summary>
        public bool IsProviderUser { get; set; }
    }

    /// <summary>نوع ورود کاربر به ناحیه‌ی Companion بدون هدر جانشینی.</summary>
    public enum CompanionPanelEntry
    {
        None = 0,
        /// <summary>ارائه‌دهنده یا مالک یک مرکز: همه‌ی endpointها با هویت خودش (چک مالکیت هر endpoint داده را محدود می‌کند).</summary>
        Owner = 1,
        /// <summary>فقط پرسنل/اپراتور یک مرکز: فقط endpointهای شخصی و اپراتوری (ProviderEndpointPolicy.StaffAllowed).</summary>
        StaffOnly = 2,
    }

    public interface ICompanionPanelAccessService
    {
        Task<CompanionPanelAccessVDto> GetAsync(long userId);
        /// <summary>برای فیلتر ناحیه‌ی Companion: فقط نتیجه‌ی مثبت ۶۰ ثانیه کش می‌شود تا کاربر تازه‌تأییدشده معطل نماند.</summary>
        Task<bool> CanEnterAsync(long userId);
        /// <summary>مثل CanEnterAsync ولی مالک را از پرسنلِ صرف جدا می‌کند؛ فقط نتیجه‌ی مثبت ۶۰ ثانیه کش می‌شود.</summary>
        Task<CompanionPanelEntry> EntryAsync(long userId);
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
            var isProvider = await _context.Providers.AsNoTracking().AnyAsync(p => p.UserId == userId && !p.Deleted && p.Active && p.Approved);

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

            var isProviderUser = await _context.ProviderUsers.AsNoTracking().AnyAsync(u =>
                u.UserId == userId && !u.Deleted && u.Active && u.UserAccept == true && !u.ProviderUserRole.Deleted &&
                !u.Provider.Deleted && u.Provider.Active && u.Provider.Approved && u.Provider.UserId != userId);

            return new CompanionPanelAccessVDto
            {
                IsProvider = isProvider,
                OwnsCenter = ownsCenter,
                IsStaff = isStaff,
                IsProviderUser = isProviderUser,
                CanEnter = isProvider || ownsCenter || isStaff,
            };
        }

        public async Task<CompanionPanelEntry> EntryAsync(long userId)
        {
            var key = "companion-panel-entry:" + userId;
            if (_cache.TryGetValue(key, out CompanionPanelEntry cached) && cached != CompanionPanelEntry.None)
                return cached;

            var access = await GetAsync(userId);
            var entry = access.IsProvider || access.OwnsCenter ? CompanionPanelEntry.Owner
                : access.IsStaff ? CompanionPanelEntry.StaffOnly
                : CompanionPanelEntry.None;
            if (entry != CompanionPanelEntry.None)
                _cache.Set(key, entry, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = PositiveCacheTtl, Size = 1 });
            return entry;
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
