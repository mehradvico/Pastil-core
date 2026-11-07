using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Setting.NoticeSrv
{
    /// <summary>
    /// اعلان ادمین (زنگوله‌ی پنل) وقتی ارائه‌دهنده چیزی ثبت/ویرایش می‌کند که تا تأیید ادمین غیرفعال می‌ماند:
    /// کلینیک/مربی/آرایشگاه/مدرسه، خدمتِ تازه‌ی مرکز و بسته‌ی مشاوره. ویرایش خودِ ادمین اعلان نمی‌سازد
    /// (NoticeEventService درخواست‌های ناحیه‌ی admin را رد می‌کند). هیچ متدی خطا پرت نمی‌کند.
    /// ردیف‌های NoticeType: scripts/VenueReviewNoticeTypes.sql
    /// </summary>
    public interface IVenueReviewNotifier
    {
        /// <param name="kind">Clinic / Coach / Groomer / School</param>
        Task VenueSubmittedAsync(string kind, long venueId, string venueName, long? actorUserId);
        /// <param name="kind">Clinic / Coach / Groomer</param>
        Task VenueUpdatedAsync(string kind, long venueId, string venueName, long? actorUserId);
        /// <param name="kind">Clinic / Coach / Groomer</param>
        Task ServiceSubmittedAsync(string kind, long venueId, long serviceId, long? actorUserId);
        /// <param name="ownerKind">Clinic / Coach</param>
        Task ConsultationPackageAsync(bool isNew, string ownerKind, long ownerId, long packageId, string packageName, long? actorUserId);
    }

    public class VenueReviewNotifier : IVenueReviewNotifier
    {
        private readonly INoticeService _noticeService;
        private readonly ILogger<VenueReviewNotifier> _logger;

        public VenueReviewNotifier(INoticeService noticeService, ILogger<VenueReviewNotifier> logger)
        {
            _noticeService = noticeService;
            _logger = logger;
        }

        public Task VenueSubmittedAsync(string kind, long venueId, string venueName, long? actorUserId)
        {
            var label = $"{kind}.Submitted";
            return SafeCreateAsync(new NoticeCreateDto
            {
                Label = label,
                ActorUserId = actorUserId,
                ReferenceType = kind,
                ReferenceId = venueId,
                DeduplicationKey = $"{label}:{venueId}",
                Metadata = new Dictionary<string, string> { { "venueName", venueName ?? string.Empty } }
            });
        }

        public Task VenueUpdatedAsync(string kind, long venueId, string venueName, long? actorUserId)
        {
            var label = $"{kind}.Updated";
            return SafeCreateAsync(new NoticeCreateDto
            {
                Label = label,
                ActorUserId = actorUserId,
                ReferenceType = kind,
                ReferenceId = venueId,
                DeduplicationKey = $"{label}:{venueId}:{DateTime.UtcNow.Ticks}",
                Metadata = new Dictionary<string, string> { { "venueName", venueName ?? string.Empty } }
            });
        }

        public Task ServiceSubmittedAsync(string kind, long venueId, long serviceId, long? actorUserId)
        {
            return SafeCreateAsync(new NoticeCreateDto
            {
                Label = NoticeTypeLabels.VenueServiceSubmitted,
                ActorUserId = actorUserId,
                ReferenceType = "VenueService",
                ReferenceId = serviceId,
                DeduplicationKey = $"{NoticeTypeLabels.VenueServiceSubmitted}:{kind}:{serviceId}",
                Metadata = new Dictionary<string, string> { { "venueKind", kind }, { "venueId", venueId.ToString() } }
            });
        }

        public Task ConsultationPackageAsync(bool isNew, string ownerKind, long ownerId, long packageId, string packageName, long? actorUserId)
        {
            var label = isNew ? NoticeTypeLabels.ConsultationPackageSubmitted : NoticeTypeLabels.ConsultationPackageUpdated;
            return SafeCreateAsync(new NoticeCreateDto
            {
                Label = label,
                ActorUserId = actorUserId,
                ReferenceType = "ConsultationPackage",
                ReferenceId = packageId,
                DeduplicationKey = isNew ? $"{label}:{packageId}" : $"{label}:{packageId}:{DateTime.UtcNow.Ticks}",
                Metadata = new Dictionary<string, string>
                {
                    { "venueKind", ownerKind },
                    { "venueId", ownerId.ToString() },
                    { "packageName", packageName ?? string.Empty }
                }
            });
        }

        private async Task SafeCreateAsync(NoticeCreateDto dto)
        {
            try
            {
                var result = await _noticeService.CreateAsync(dto);
                if (result != null && !result.IsSuccess)
                    _logger.LogWarning("Admin review notice {Label} for {ReferenceType} {ReferenceId} was not created.", dto.Label, dto.ReferenceType, dto.ReferenceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin review notice {Label} for {ReferenceType} {ReferenceId} failed.", dto.Label, dto.ReferenceType, dto.ReferenceId);
            }
        }
    }
}
