using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveSrv.Dto;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveSrv.Iface
{
    // تغییر زمان رزرو (کلینیک / مربی / آرایشگاه) توسط نماینده یا ادمین؛ پیامک به کاربر و راننده‌ی سفر متصل.
    // طراحی: backend/Docs/COMPANION_RESERVE_RESCHEDULE_FA.md
    public interface ICompanionReserveRescheduleService
    {
        // companionId: برای نماینده (مالکیت چک می‌شود)؛ برای ادمین null
        Task<BaseResultDto<CompanionReserveRescheduleSlotsVDto>> GetSlotsAsync(long reserveId, DateTime date, long? companionId, CancellationToken cancellationToken = default);
        Task<BaseResultDto<CompanionReserveRescheduleResultVDto>> RescheduleAsync(CompanionReserveRescheduleDto dto, long actorUserId, int actorKind, long? companionId, CancellationToken cancellationToken = default);
        Task<BaseResultDto<List<CompanionReserveRescheduleVDto>>> GetHistoryAsync(long reserveId, long? companionId, CancellationToken cancellationToken = default);
    }
}
