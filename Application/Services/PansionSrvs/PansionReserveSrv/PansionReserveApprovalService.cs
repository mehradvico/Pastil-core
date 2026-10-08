using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.PansionSrvs.PansionReserveSrv.Iface;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.TripSrv.TripSrv.Iface;
using Entities.Entities.PansionField;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionReserveSrv
{
    public class PansionReserveApprovalService : IPansionReserveApprovalService
    {
        private readonly IDataBaseContext _context;
        private readonly IWalletService _walletService;
        private readonly IPushNotificationService _pushService;
        private readonly IClubPointIntegrationService _clubPoints;
        private readonly ITripService _tripService;
        private readonly ILogger<PansionReserveApprovalService> _logger;

        public PansionReserveApprovalService(
            IDataBaseContext context,
            IWalletService walletService,
            IPushNotificationService pushService,
            IClubPointIntegrationService clubPoints,
            ITripService tripService,
            ILogger<PansionReserveApprovalService> logger)
        {
            _context = context;
            _walletService = walletService;
            _pushService = pushService;
            _clubPoints = clubPoints;
            _tripService = tripService;
            _logger = logger;
        }

        public async Task<BaseResultDto> ApproveAsync(long reserveId, long? companionId, CancellationToken cancellationToken = default)
        {
            var reserve = await LoadAsync(reserveId, cancellationToken);
            var check = Check(reserve, companionId);
            if (check != null)
                return check;

            reserve.OwnerDecision = (int)PansionReserveOwnerDecisionEnum.Approved;
            reserve.OwnerDecisionDate = DateTime.Now;
            reserve.OwnerDecisionReason = null;
            await _context.SaveChangesAsync(cancellationToken);

            await SafeAsync(() => _pushService.SendPushAsync(
                PushTypeEnum.PushPansionReserveApproved, reserve.BookerId,
                token1: reserve.Pansion?.Name, token2: reserve.UserPet?.Pet?.Name, token3: reserve.Id.ToString()),
                reserve.Id, "approved push");

            return new BaseResultDto(true, Resource.Notification.PansionReserveApprovedSuccessfully);
        }

        public async Task<BaseResultDto> RejectAsync(long reserveId, string reason, long? companionId, CancellationToken cancellationToken = default)
        {
            reason = reason?.Trim();
            if (string.IsNullOrWhiteSpace(reason))
                return new BaseResultDto(false, Resource.Notification.PansionReserveRejectReasonRequired);

            return await CancelWithRefundAsync(reserveId, companionId, PansionReserveOwnerDecisionEnum.Rejected, reason, cancellationToken);
        }

        public async Task ExpireOverdueAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            var pending = (int)PansionReserveOwnerDecisionEnum.Pending;
            var ids = await _context.PansionReserves.AsNoTracking()
                .Where(r => r.OwnerDecision == pending && r.IsReserved && !r.IsCancel
                            && r.OwnerApprovalDeadline != null && r.OwnerApprovalDeadline <= now)
                .Select(r => r.Id)
                .Take(100)
                .ToListAsync(cancellationToken);

            foreach (var id in ids)
            {
                try
                {
                    var result = await CancelWithRefundAsync(id, null, PansionReserveOwnerDecisionEnum.Expired,
                        Resource.Notification.PansionReserveExpiredReason, cancellationToken);
                    if (!result.IsSuccess)
                        _logger.LogWarning("Expiring pansion reserve {ReserveId} failed: {Message}", id, result.Messages?.FirstOrDefault()?.Item1);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Expiring pansion reserve {ReserveId} failed.", id);
                }
            }
        }

        // لغو رزرو + برگشت کل مبلغ پرداخت‌شده به کیف پول کاربر (در یک تراکنش؛ اگر برگشت پول نشد، لغو هم نمی‌شود)
        private async Task<BaseResultDto> CancelWithRefundAsync(
            long reserveId, long? companionId, PansionReserveOwnerDecisionEnum decision, string reason, CancellationToken ct)
        {
            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);

            var reserve = await LoadAsync(reserveId, ct);
            var check = Check(reserve, companionId);
            if (check != null)
            {
                await transaction.RollbackAsync(ct);
                return check;
            }

            var now = DateTime.Now;
            reserve.OwnerDecision = (int)decision;
            reserve.OwnerDecisionDate = now;
            reserve.OwnerDecisionReason = reason;
            reserve.IsCancel = true;
            reserve.CancelDate = now;
            reserve.CancelDetail = reason;
            await _context.SaveChangesAsync(ct);

            if (!await RefundAsync(reserve, ct))
            {
                await transaction.RollbackAsync(ct);
                return new BaseResultDto(false, Resource.Notification.PansionReserveRefundFailed);
            }

            await transaction.CommitAsync(ct);

            await SafeAsync(() => _clubPoints.PansionReserveReversedAsync(reserve.BookerId, reserve.Id, ct), reserve.Id, "club points reversal");
            await SafeAsync(() => _tripService.CancelLinkedTripForPansionReserveAsync(reserve.Id), reserve.Id, "linked trip cancel");

            var pansionName = reserve.Pansion?.Name;
            await SafeAsync(() => decision == PansionReserveOwnerDecisionEnum.Rejected
                ? _pushService.SendPushAsync(PushTypeEnum.PushPansionReserveRejected, reserve.BookerId,
                    token1: pansionName, token2: reason, token3: reserve.Id.ToString())
                : _pushService.SendPushAsync(PushTypeEnum.PushPansionReserveExpired, reserve.BookerId,
                    token1: pansionName, token3: reserve.Id.ToString()),
                reserve.Id, "cancel push");

            return new BaseResultDto(true, Resource.Notification.PansionReserveRejectedSuccessfully);
        }

        private async Task<bool> RefundAsync(PansionReserve reserve, CancellationToken ct)
        {
            var amount = reserve.PaymentPrice;
            if (amount <= 0)
                return true;

            var ledgerName = PansionReserveApprovalRules.RefundLedgerName(reserve.Id);
            // یکتایی با Name: اجرای دوباره (تلاش مجدد/ریس) پول را دوباره برنمی‌گرداند. ProductOrderId/PansionReserveId روی ردیف برگشت
            // عمداً پر نمی‌شود چون ردیف کسر اصلی از قبل همان ایندکس یکتا را دارد.
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
                Painding = false
            });
            return result.IsSuccess;
        }

        private Task<PansionReserve> LoadAsync(long reserveId, CancellationToken ct) =>
            _context.PansionReserves
                .Include(r => r.Pansion).ThenInclude(p => p.Companion)
                .Include(r => r.UserPet).ThenInclude(p => p.Pet)
                .AsTracking()
                .FirstOrDefaultAsync(r => r.Id == reserveId, ct);

        private static BaseResultDto Check(PansionReserve reserve, long? companionId)
        {
            if (reserve == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (companionId.HasValue && reserve.Pansion?.CompanionId != companionId.Value)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            if (!PansionReserveApprovalRules.CanDecide(reserve.IsReserved, reserve.IsCancel, reserve.OwnerDecision))
                return new BaseResultDto(false, Resource.Notification.PansionReserveApprovalNotPending);
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
                _logger.LogError(ex, "Post-commit action {Action} failed for pansion reserve {ReserveId}.", name, reserveId);
            }
        }
    }
}
