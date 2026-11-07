using Entities.Entities.ProviderField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionOwnershipSrv
{
    /// <summary>
    /// مالکیت پانسیون/رزرو/پت/تصویر/نظر برای پنل/اپ ارائه‌دهنده (۲۰۲۶-۱۱-۰۳، مثل مدرسه): مالک پانسیون «کاربرِ ارائه‌دهنده‌ی» آن است (Pansion.ProviderId)؛
    /// برای پانسیون‌های قدیمی مالکِ نمایندگی (Pansion.CompanionId → Companion.OwnerId) هم مثل قبل مالک حساب می‌شود.
    /// همیشه از دیتابیس سنجیده می‌شود، نه از توکن. هر متد شناسه‌ی «پانسیونِ» صاحب منبع را برمی‌گرداند (یا null = مالک نیست).
    /// approvedOnly: عملیات نوشتن فقط برای ارائه‌دهنده/نمایندگی فعال و تأییدشده؛ خواندن مال خود بدون آن هم مجاز است.
    /// پانسیون حذف‌شده هم شمرده می‌شود (رزروها و تسویه‌اش باید دیده شوند)؛ عملیات روی خود پانسیون Deleted را جدا رد می‌کند.
    /// </summary>
    public interface IPansionOwnershipService
    {
        /// <summary>
        /// مالک پانسیون جدید: ارائه‌دهنده‌ی فعال/تأییدشده با نقش تأییدشده‌ی Pansion (نمایندگی اختیاری)، وگرنه نمایندگیِ قدیمیِ کاربر.
        /// Error = متن پیام وقتی هیچ‌کدام نیست.
        /// </summary>
        Task<NewPansionOwner> ResolveOwnerForNewPansionAsync(long userId);
        /// <summary>کاربر مالک این پانسیون است؟ (برای فیلتر فهرست‌ها در سرویس‌ها)</summary>
        IQueryable<long> OwnedPansionIds(long userId, bool approvedOnly = true);
        Task<long?> PansionOfPansionAsync(long pansionId, long userId, bool approvedOnly = true);
        Task<long?> PansionOfReserveAsync(long reserveId, long userId, bool approvedOnly = true);
        Task<long?> PansionOfPetAsync(long pansionPetId, long userId, bool approvedOnly = true);
        Task<long?> PansionOfPictureAsync(long pansionPictureId, long userId, bool approvedOnly = true);
    }

    public sealed record NewPansionOwner(long? ProviderId, long? CompanionId, string Error)
    {
        public bool Ok => Error == null;
    }

    public class PansionOwnershipService : IPansionOwnershipService
    {
        private readonly IDataBaseContext _context;

        public PansionOwnershipService(IDataBaseContext context) => _context = context;

        public async Task<NewPansionOwner> ResolveOwnerForNewPansionAsync(long userId)
        {
            var providerId = await ProviderWithPansionRole(_context, userId).FirstOrDefaultAsync();
            if (providerId.HasValue)
                return new NewPansionOwner(providerId, null, null);

            // بدون نقش پانسیون: مسیر قدیمی (پانسیون زیر نمایندگی)؛ PansionService ارائه‌دهنده‌ی همان نمایندگی را هم ثبت می‌کند تا تسویه درست برود
            var companion = await _context.Companions.AsNoTracking()
                .Where(c => c.OwnerId == userId && !c.Deleted && c.Active && c.Approved)
                .OrderBy(c => c.Id)
                .Select(c => new { c.Id, c.ProviderId })
                .FirstOrDefaultAsync();
            if (companion != null)
                return new NewPansionOwner(null, companion.Id, null);
            return new NewPansionOwner(null, null, Resource.Notification.PansionNeedsProviderRole);
        }

        /// <summary>ارائه‌دهنده‌ی فعال/تأییدشده‌ی کاربر که نقش تأییدشده‌ی «پانسیون» دارد.</summary>
        public static IQueryable<long?> ProviderWithPansionRole(IDataBaseContext context, long userId) =>
            context.Providers.AsNoTracking()
                .Where(p => p.UserId == userId && !p.Deleted && p.Active && p.Approved)
                .Where(p => context.ProviderRoleAssignments.Any(a => a.ProviderId == p.Id && a.Approved && !a.Deleted
                    && a.ProviderRole.Label == ProviderRoleLabels.Pansion && a.ProviderRole.Active && !a.ProviderRole.Deleted))
                .Select(p => (long?)p.Id);

        /// <summary>
        /// پانسیون‌های این کاربر: ارائه‌دهنده‌اش (Pansion.ProviderId) یا نمایندگی‌اش (Pansion.CompanionId).
        /// انتقال به ارائه‌دهنده‌ی دیگر (ProviderVenue) نمایندگیِ غیرِ همان ارائه‌دهنده را جدا می‌کند، پس مالک قبلی دسترسی نمی‌ماند.
        /// </summary>
        public static IQueryable<long> OwnedPansionIds(IDataBaseContext context, long userId, bool approvedOnly = true) =>
            context.Pansions.AsNoTracking()
                .Where(s =>
                    (s.ProviderId != null && s.Provider.UserId == userId && !s.Provider.Deleted
                        && (!approvedOnly || (s.Provider.Active && s.Provider.Approved)))
                    || (s.CompanionId != null && s.Companion.OwnerId == userId && !s.Companion.Deleted
                        && (!approvedOnly || (s.Companion.Active && s.Companion.Approved))))
                .Select(s => s.Id);

        public IQueryable<long> OwnedPansionIds(long userId, bool approvedOnly = true) => OwnedPansionIds(_context, userId, approvedOnly);

        public Task<long?> PansionOfPansionAsync(long pansionId, long userId, bool approvedOnly = true) =>
            OwnedPansionIds(userId, approvedOnly).Where(id => id == pansionId).Select(id => (long?)id).FirstOrDefaultAsync();

        public Task<long?> PansionOfReserveAsync(long reserveId, long userId, bool approvedOnly = true) =>
            _context.PansionReserves.AsNoTracking()
                .Where(r => r.Id == reserveId && OwnedPansionIds(userId, approvedOnly).Contains(r.PansionId))
                .Select(r => (long?)r.PansionId).FirstOrDefaultAsync();

        public Task<long?> PansionOfPetAsync(long pansionPetId, long userId, bool approvedOnly = true) =>
            _context.PansionPets.AsNoTracking()
                .Where(p => p.Id == pansionPetId && OwnedPansionIds(userId, approvedOnly).Contains(p.PansionId))
                .Select(p => (long?)p.PansionId).FirstOrDefaultAsync();

        public Task<long?> PansionOfPictureAsync(long pansionPictureId, long userId, bool approvedOnly = true) =>
            _context.PansionPictures.AsNoTracking()
                .Where(p => p.Id == pansionPictureId && OwnedPansionIds(userId, approvedOnly).Contains(p.PansionId))
                .Select(p => (long?)p.PansionId).FirstOrDefaultAsync();
    }
}
