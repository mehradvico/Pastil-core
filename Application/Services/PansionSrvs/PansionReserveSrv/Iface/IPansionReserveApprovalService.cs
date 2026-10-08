using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionReserveSrv.Iface
{
    // تأیید مرکز بعد از پرداخت: رد یا بی‌پاسخ ماندن = لغو رزرو + برگشت کامل مبلغ به کیف پول کاربر.
    // طراحی: backend/Docs/PANSION_RESERVE_OWNER_APPROVAL_FA.md
    public interface IPansionReserveApprovalService
    {
        // companionId: برای مرکز (مالکیت چک می‌شود)؛ برای ادمین null
        Task<BaseResultDto> ApproveAsync(long reserveId, long? companionId, CancellationToken cancellationToken = default);
        Task<BaseResultDto> RejectAsync(long reserveId, string reason, long? companionId, CancellationToken cancellationToken = default);

        // job هر چند دقیقه: رزروهایی که مهلت پاسخشان گذشته لغو و پول برگردانده می‌شود
        Task ExpireOverdueAsync(CancellationToken cancellationToken = default);
    }
}
