using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Services.CoachSrvs.CoachReserveSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Entities.Entities.CoachField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachReserveSrv
{
    /// <summary>تأیید/رد توسط مربی، لغو توسط کاربر یا ادمین، و لغو خودکار رزروهای بی‌پاسخ؛ همه با برگشت وجه اتمی به کیف پول.</summary>
    public class CoachReserveApprovalService : ICoachReserveApprovalService
    {
        private readonly IDataBaseContext _context;
        private readonly IWalletService _walletService;
        private readonly ICoachNotifier _notifier;
        private readonly ILogger<CoachReserveApprovalService> _logger;

        public CoachReserveApprovalService(
            IDataBaseContext context,
            IWalletService walletService,
            ICoachNotifier notifier,
            ILogger<CoachReserveApprovalService> logger)
        {
            _context = context;
            _walletService = walletService;
            _notifier = notifier;
            _logger = logger;
        }

        public async Task<BaseResultDto> ApproveAsync(long reserveId, long? ownerUserId, CancellationToken cancellationToken = default)
        {
            var reserve = await LoadAsync(reserveId, cancellationToken);
            var check = CheckDecidable(reserve, ownerUserId);
            if (check != null)
                return check;

            reserve.OwnerDecision = (int)CoachReserveOwnerDecisionEnum.Approved;
            reserve.OwnerDecisionDate = DateTime.Now;
            reserve.OwnerDecisionReason = null;
            await _context.SaveChangesAsync(cancellationToken);

            await _notifier.ReserveApprovedAsync(reserve.Id);
            return new BaseResultDto(true, Resource.Notification.CoachReserveApprovedSuccessfully);
        }

        public async Task<BaseResultDto> RejectAsync(long reserveId, string reason, long? ownerUserId, CancellationToken cancellationToken = default)
        {
            reason = reason?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
                return new BaseResultDto(false, Resource.Notification.CoachReserveRejectReasonRequired);
            return await CancelWithRefundAsync(reserveId, ownerUserId, CoachReserveOwnerDecisionEnum.Rejected, reason, cancellationToken);
        }

        public async Task ExpireOverdueAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            var pending = (int)CoachReserveOwnerDecisionEnum.Pending;
            var ids = await _context.CoachReserves.AsNoTracking()
                .Where(r => r.OwnerDecision == pending && r.IsReserved && !r.IsCancel
                            && r.OwnerApprovalDeadline != null && r.OwnerApprovalDeadline <= now)
                .OrderBy(r => r.OwnerApprovalDeadline)
                .Select(r => r.Id)
                .Take(100)
                .ToListAsync(cancellationToken);
            foreach (var id in ids)
            {
                try
                {
                    var result = await CancelWithRefundAsync(id, null, CoachReserveOwnerDecisionEnum.Expired,
                        Resource.Notification.CoachReserveExpiredReason, cancellationToken);
                    if (!result.IsSuccess)
                        _logger.LogWarning("Expiring coach reserve {ReserveId} failed: {Message}", id, result.Messages?.FirstOrDefault()?.Item1);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Expiring coach reserve {ReserveId} failed.", id);
                }
            }
        }

        public async Task<BaseResultDto> CancelByBookerAsync(long reserveId, long bookerId, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
            var reserve = await LoadAsync(reserveId, cancellationToken);
            if (reserve == null || reserve.BookerId != bookerId)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }
            var now = DateTime.Now;
            if (reserve.IsCancel || reserve.Status == (int)CoachReserveStatusEnum.Complete || (reserve.IsReserved && reserve.DoDate <= now))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new BaseResultDto(false, Resource.Notification.CoachReserveCannotCancel);
            }

            // پرداخت‌نشده: فقط بسته می‌شود. پرداخت‌شده: اگر هنوز منتظر تأیید مربی است یا در مهلت لغو رایگان است، کل مبلغ برمی‌گردد.
            var refund = reserve.IsReserved &&
                         (CoachReserveRules.IsPending(reserve.OwnerDecision) || CoachReserveRules.IsFreeCancel(now, reserve.DoDate));
            ApplyCancel(reserve, now, null, byBooker: true);
            await _context.SaveChangesAsync(cancellationToken);

            if (refund && !await RefundAsync(reserve, cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new BaseResultDto(false, Resource.Notification.CoachReserveRefundFailed);
            }
            await transaction.CommitAsync(cancellationToken);

            if (reserve.IsReserved)
                await _notifier.ReserveCancelledByBookerAsync(reserve.Id);
            return new BaseResultDto(true, !reserve.IsReserved || refund
                ? Resource.Notification.CoachReserveCancelledWithRefund
                : Resource.Notification.CoachReserveCancelledNoRefund);
        }

        public async Task<BaseResultDto> CancelByAdminAsync(long reserveId, string reason, bool refund, CancellationToken cancellationToken = default)
        {
            reason = reason?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
                return new BaseResultDto(false, Resource.Notification.PleaseEnterCancelDetail);

            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
            var reserve = await LoadAsync(reserveId, cancellationToken);
            if (reserve == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }
            if (reserve.IsCancel || reserve.Status == (int)CoachReserveStatusEnum.Complete)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new BaseResultDto(false, Resource.Notification.CoachReserveCannotCancel);
            }

            ApplyCancel(reserve, DateTime.Now, reason, byBooker: false);
            await _context.SaveChangesAsync(cancellationToken);
            if (refund && reserve.IsReserved && !await RefundAsync(reserve, cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new BaseResultDto(false, Resource.Notification.CoachReserveRefundFailed);
            }
            await transaction.CommitAsync(cancellationToken);
            if (reserve.IsReserved)
                await _notifier.ReserveCancelledByAdminAsync(reserve.Id, reason);
            return new BaseResultDto(true, Resource.Notification.Success);
        }

        // ------------------------------------------------------------------ داخلی

        // لغو + برگشت کل مبلغ پرداخت‌شده در یک تراکنش؛ اگر برگشت پول نشد، لغو هم نمی‌شود.
        private async Task<BaseResultDto> CancelWithRefundAsync(
            long reserveId, long? ownerUserId, CoachReserveOwnerDecisionEnum decision, string reason, CancellationToken ct)
        {
            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);
            var reserve = await LoadAsync(reserveId, ct);
            var check = CheckDecidable(reserve, ownerUserId);
            if (check != null)
            {
                await transaction.RollbackAsync(ct);
                return check;
            }

            var now = DateTime.Now;
            reserve.OwnerDecision = (int)decision;
            reserve.OwnerDecisionDate = now;
            reserve.OwnerDecisionReason = reason;
            ApplyCancel(reserve, now, reason, byBooker: false);
            await _context.SaveChangesAsync(ct);
            if (!await RefundAsync(reserve, ct))
            {
                await transaction.RollbackAsync(ct);
                return new BaseResultDto(false, Resource.Notification.CoachReserveRefundFailed);
            }
            await transaction.CommitAsync(ct);

            if (decision == CoachReserveOwnerDecisionEnum.Rejected)
                await _notifier.ReserveRejectedAsync(reserve.Id, reason);
            else
                await _notifier.ReserveExpiredAsync(reserve.Id);
            return new BaseResultDto(true, Resource.Notification.CoachReserveRejectedSuccessfully);
        }

        private static void ApplyCancel(CoachReserve reserve, DateTime now, string reason, bool byBooker)
        {
            reserve.IsCancel = true;
            reserve.CancelDate = now;
            reserve.CancelDetail = reason;
            reserve.CancelledByBooker = byBooker;
        }

        // برگشت کل مبلغ پرداخت‌شده به کیف پول؛ کلید دفتر (Name) یکتاست پس اجرای دوباره (تلاش مجدد/ریس) پول را دوباره برنمی‌گرداند.
        private async Task<bool> RefundAsync(CoachReserve reserve, CancellationToken ct)
        {
            var amount = reserve.PaymentPrice;
            if (amount <= 0)
                return true;
            var ledgerName = CoachReserveRules.RefundLedgerName(reserve.Id);
            var exists = await _context.Wallets.AsNoTracking()
                .AnyAsync(w => w.Name == ledgerName && w.UserId == reserve.BookerId && !w.Deleted, ct);
            if (exists)
                return true;
            var result = await _walletService.InsertAsyncDto(new WalletDto
            {
                Name = ledgerName,
                Amount = amount,
                IsIncrease = true,
                UserId = reserve.BookerId,
                Painding = false,
            });
            return result.IsSuccess;
        }

        private Task<CoachReserve> LoadAsync(long reserveId, CancellationToken ct) =>
            _context.CoachReserves
                .Include(r => r.Coach)
                .Include(r => r.UserPet)
                .AsTracking()
                .FirstOrDefaultAsync(r => r.Id == reserveId, ct);

        private static BaseResultDto CheckDecidable(CoachReserve reserve, long? ownerUserId)
        {
            if (reserve == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (ownerUserId.HasValue && reserve.Coach?.OwnerId != ownerUserId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            if (!CoachReserveRules.CanDecide(reserve.IsReserved, reserve.IsCancel, reserve.OwnerDecision))
                return new BaseResultDto(false, Resource.Notification.CoachReserveApprovalNotPending);
            return null;
        }

        private async Task SafeAsync(Func<Task> action, long reserveId, string name)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Post-commit action {Action} failed for coach reserve {ReserveId}.", name, reserveId);
            }
        }
    }
}
