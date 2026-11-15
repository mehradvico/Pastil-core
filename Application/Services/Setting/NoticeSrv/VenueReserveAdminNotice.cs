using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Setting.NoticeSrv
{
    /// <summary>
    /// اعلان ادمین «رزرو جدید» برای رزروهای کلینیک/مربی/آرایشگاه (بعد از پرداخت، همان لحظه‌ی ReservePaidAsync).
    /// ردیف‌های NoticeType: scripts/AdminNoticeImportance.sql. هرگز خطا پرت نمی‌کند.
    /// </summary>
    public static class VenueReserveAdminNotice
    {
        public static async Task CreateAsync(INoticeEventService notices, ILogger logger, string kind, long reserveId, long bookerId, string bookerName, string venueName, DateTime doDate)
        {
            if (notices == null)
                return;
            var label = $"{kind}Reserve.Registered";
            try
            {
                await notices.CreateAsync(new NoticeCreateDto
                {
                    Label = label,
                    ActorUserId = bookerId,
                    ReferenceType = $"{kind}Reserve",
                    ReferenceId = reserveId,
                    DeduplicationKey = $"{label}:{reserveId}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "userName", bookerName ?? string.Empty },
                        { "venueName", venueName ?? string.Empty },
                        { "reserveDate", doDate.ToString("yyyy/MM/dd HH:mm") }
                    }
                });
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Admin notice {Label} for reserve {ReserveId} failed.", label, reserveId);
            }
        }
    }
}
