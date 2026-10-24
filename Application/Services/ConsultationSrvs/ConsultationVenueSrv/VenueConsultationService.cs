using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.ConsultationOnlineSrv;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationBookingSrv.Iface;
using Application.Services.ConsultationSrvs.ConsultationOwnerSrv;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationVenueSrv
{
    public enum VenueKind
    {
        Clinic = 1,
        Coach = 2,
    }

    /// <summary>
    /// مشاوره‌ی آنلاین فوری (بسته‌ی چت/تماس، خرید، جلسه، «من آنلاینم»، برنامه‌ی رزرو) برای مراکز مستقل کلینیک و مربی، با شناسه‌ی خودِ مرکز.
    /// بسته/خرید/بازه‌ی رزرو مستقیماً روی ClinicId / CoachId کلید می‌خورند (ConsultationOwner)؛ دیگر Companion «لنگر» وجود ندارد.
    /// تیم و «آنلاینم» از ClinicUsers / CoachUsers می‌آید، تسویه در استخر ارائه‌دهنده‌ی همان مرکز (SettlementConsultation).
    /// </summary>
    public interface IVenueConsultationService
    {
        // عمومی: بسته‌های قابل خرید یک مرکز (مرکز فعال و تأییدشده)
        Task<BaseResultDto<List<ConsultationPackagePublicVDto>>> GetPublicAsync(VenueKind kind, long venueId);

        // مالک/عضو فعال مرکز
        Task<BaseResultDto<List<ConsultationPackageItemDto>>> GetListAsync(VenueKind kind, long venueId, long userId);
        Task<BaseResultDto<ConsultationPackageItemDto>> CreateAsync(VenueKind kind, long venueId, long userId, ConsultationPackageItemDto dto);
        Task<BaseResultDto<ConsultationPackageItemDto>> UpdateAsync(VenueKind kind, long venueId, long userId, ConsultationPackageItemDto dto);
        Task<BaseResultDto> DeleteAsync(VenueKind kind, long venueId, long userId, long id);

        Task<BaseResultDto<ConsultationOnlineStatusDto>> GetOnlineAsync(VenueKind kind, long venueId, long userId);
        Task<BaseResultDto<ConsultationOnlineStatusDto>> SetOnlineAsync(VenueKind kind, long venueId, long userId, bool online);

        // برنامه‌ی هفتگی رزرو مشاوره (پکیج‌های «قابل رزرو»)
        Task<BaseResultDto<List<ConsultationAvailabilityWindowDto>>> GetAvailabilityAsync(VenueKind kind, long venueId, long userId);
        Task<BaseResultDto<List<ConsultationAvailabilityWindowDto>>> SaveAvailabilityAsync(VenueKind kind, long venueId, long userId, List<ConsultationAvailabilityWindowDto> windows);
    }

    public class VenueConsultationService : IVenueConsultationService
    {
        private readonly IDataBaseContext _context;
        private readonly IConsultationPackageService _packages;
        private readonly IConsultationOnlineService _online;
        private readonly IConsultationBookingService _booking;

        public VenueConsultationService(IDataBaseContext context, IConsultationPackageService packages, IConsultationOnlineService online,
            IConsultationBookingService booking)
        {
            _context = context;
            _packages = packages;
            _online = online;
            _booking = booking;
        }

        private sealed record Venue(long Id, bool Active, bool Approved);

        private static ConsultationOwner OwnerOf(VenueKind kind, long venueId) =>
            kind == VenueKind.Clinic ? ConsultationOwner.Clinic(venueId) : ConsultationOwner.Coach(venueId);

        /// <summary>مرکز (حذف‌نشده)؛ با userId فقط اگر مالک یا عضو فعال و تأییدشده‌ی تیم باشد، وگرنه null.</summary>
        private async Task<Venue> LoadVenueAsync(VenueKind kind, long venueId, long? userId)
        {
            if (kind == VenueKind.Clinic)
                return await _context.Clinics.AsNoTracking()
                    .Where(v => v.Id == venueId && !v.Deleted
                                && (userId == null || v.OwnerId == userId || v.ClinicUsers.Any(u => u.UserId == userId && !u.Deleted && u.Active && u.UserAccept == true)))
                    .Select(v => new Venue(v.Id, v.Active, v.Approved))
                    .FirstOrDefaultAsync();
            return await _context.Coaches.AsNoTracking()
                .Where(v => v.Id == venueId && !v.Deleted
                            && (userId == null || v.OwnerId == userId || v.CoachUsers.Any(u => u.UserId == userId && !u.Deleted && u.Active && u.UserAccept == true)))
                .Select(v => new Venue(v.Id, v.Active, v.Approved))
                .FirstOrDefaultAsync();
        }

        /// <summary>دسترسی کاربر به مرکز بررسی می‌شود؛ برای نوشتن (ساخت/ویرایش) مرکز باید فعال و تأییدشده هم باشد.</summary>
        private async Task<(ConsultationOwner? Owner, string Error)> AuthorizeAsync(VenueKind kind, long venueId, long userId, bool requireOpen)
        {
            var venue = await LoadVenueAsync(kind, venueId, userId);
            if (venue == null)
                return (null, Resource.Notification.AccessDenied);
            if (requireOpen && (!venue.Active || !venue.Approved))
                return (null, Resource.Notification.VenueConsultationVenueNotApproved);
            return (OwnerOf(kind, venueId), null);
        }

        public async Task<BaseResultDto<List<ConsultationPackagePublicVDto>>> GetPublicAsync(VenueKind kind, long venueId)
        {
            var venue = await LoadVenueAsync(kind, venueId, null);
            if (venue == null || !venue.Active || !venue.Approved)
                return new BaseResultDto<List<ConsultationPackagePublicVDto>>(true, new List<ConsultationPackagePublicVDto>());
            return await _packages.GetPublicAsync(OwnerOf(kind, venueId));
        }

        public async Task<BaseResultDto<List<ConsultationPackageItemDto>>> GetListAsync(VenueKind kind, long venueId, long userId)
        {
            var (owner, error) = await AuthorizeAsync(kind, venueId, userId, requireOpen: false);
            return error != null
                ? new BaseResultDto<List<ConsultationPackageItemDto>>(false, error, null)
                : await _packages.GetListAsync(owner.Value);
        }

        public async Task<BaseResultDto<ConsultationPackageItemDto>> CreateAsync(VenueKind kind, long venueId, long userId, ConsultationPackageItemDto dto)
        {
            var (owner, error) = await AuthorizeAsync(kind, venueId, userId, requireOpen: true);
            return error != null
                ? new BaseResultDto<ConsultationPackageItemDto>(false, error, null)
                : await _packages.CreateAsync(owner.Value, dto);
        }

        public async Task<BaseResultDto<ConsultationPackageItemDto>> UpdateAsync(VenueKind kind, long venueId, long userId, ConsultationPackageItemDto dto)
        {
            var (owner, error) = await AuthorizeAsync(kind, venueId, userId, requireOpen: true);
            return error != null
                ? new BaseResultDto<ConsultationPackageItemDto>(false, error, null)
                : await _packages.UpdateAsync(owner.Value, dto);
        }

        public async Task<BaseResultDto> DeleteAsync(VenueKind kind, long venueId, long userId, long id)
        {
            var (owner, error) = await AuthorizeAsync(kind, venueId, userId, requireOpen: false);
            return error != null
                ? new BaseResultDto(false, error)
                : await _packages.DeleteAsync(owner.Value, id);
        }

        public async Task<BaseResultDto<ConsultationOnlineStatusDto>> GetOnlineAsync(VenueKind kind, long venueId, long userId)
        {
            var (owner, error) = await AuthorizeAsync(kind, venueId, userId, requireOpen: false);
            return error != null
                ? new BaseResultDto<ConsultationOnlineStatusDto>(false, error, null)
                : await _online.GetMineAsync(userId, owner);
        }

        public async Task<BaseResultDto<ConsultationOnlineStatusDto>> SetOnlineAsync(VenueKind kind, long venueId, long userId, bool online)
        {
            // روشن‌کردن فقط برای مرکز فعال و تأییدشده؛ خاموش‌کردن همیشه آزاد است
            var (owner, error) = await AuthorizeAsync(kind, venueId, userId, requireOpen: online);
            return error != null
                ? new BaseResultDto<ConsultationOnlineStatusDto>(false, error, null)
                : await _online.SetMineAsync(userId, owner, online);
        }

        public async Task<BaseResultDto<List<ConsultationAvailabilityWindowDto>>> GetAvailabilityAsync(VenueKind kind, long venueId, long userId)
        {
            var (owner, error) = await AuthorizeAsync(kind, venueId, userId, requireOpen: false);
            return error != null
                ? new BaseResultDto<List<ConsultationAvailabilityWindowDto>>(false, error, null)
                : await _booking.GetAvailabilityAsync(owner.Value);
        }

        public async Task<BaseResultDto<List<ConsultationAvailabilityWindowDto>>> SaveAvailabilityAsync(VenueKind kind, long venueId, long userId, List<ConsultationAvailabilityWindowDto> windows)
        {
            var (owner, error) = await AuthorizeAsync(kind, venueId, userId, requireOpen: true);
            return error != null
                ? new BaseResultDto<List<ConsultationAvailabilityWindowDto>>(false, error, null)
                : await _booking.SaveAvailabilityAsync(owner.Value, windows);
        }
    }
}
