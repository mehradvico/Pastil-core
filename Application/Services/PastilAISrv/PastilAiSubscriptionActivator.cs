using Application.Common.Dto.Result;
using Application.Services.PastilAISrv.Iface;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Entities.Entities.PastilAIField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilAISrv
{
    public class PastilAiSubscriptionActivator : IPastilAiSubscriptionActivator
    {
        private readonly IDataBaseContext _context;
        private readonly IWalletService _walletService;
        private readonly IRebateService _rebateService;

        public PastilAiSubscriptionActivator(
            IDataBaseContext context,
            IWalletService walletService,
            IRebateService rebateService)
        {
            _context = context;
            _walletService = walletService;
            _rebateService = rebateService;
        }

        public async Task<BaseResultDto> ActivateAfterPaymentAsync(long subscriptionId, long paymentId, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var subscription = await _context.PastilAiSubscriptions.AsTracking()
                .Include(x => x.Plan)
                .Include(x => x.Rebate)
                .FirstOrDefaultAsync(x => x.Id == subscriptionId, cancellationToken);
            if (subscription == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            if (subscription.Status == PastilAiSubscriptionStatus.Active)
                return new BaseResultDto(subscription.PaymentId == paymentId);

            var payment = await _context.Payments.AsNoTracking().FirstOrDefaultAsync(x =>
                x.Id == paymentId &&
                x.UserId == subscription.UserId &&
                x.IsSuccess == true &&
                x.CallBackTypeLabel == Application.Common.Enumerable.PaymentCallbackTypeEnum.PastilAI.ToString() &&
                x.CallBackId == subscriptionId.ToString(), cancellationToken);
            if (payment == null)
                return new BaseResultDto(false, Resource.Notification.Unsuccess);

            if (subscription.FromWallet && subscription.WalletPrice > 0)
            {
                var walletResult = await _walletService.InsertUpdatePastilAiSubscriptionAsync(new WalletDto
                {
                    Amount = (double)subscription.WalletPrice,
                    UserId = subscription.UserId,
                    PastilAiSubscriptionId = subscription.Id,
                    Painding = false
                }, true);
                if (!walletResult.IsSuccess)
                    return new BaseResultDto(false, Resource.Notification.InsufficientFunds);
            }

            var now = DateTime.UtcNow;
            var currentEnd = await _context.PastilAiSubscriptions.AsNoTracking()
                .Where(x => x.UserId == subscription.UserId && x.Status == PastilAiSubscriptionStatus.Active && x.EndDateUtc > now)
                .MaxAsync(x => (DateTime?)x.EndDateUtc, cancellationToken);
            subscription.PaymentId = paymentId;
            subscription.Status = PastilAiSubscriptionStatus.Active;
            subscription.StartDateUtc = now;
            subscription.EndDateUtc = (currentEnd ?? now).AddDays(subscription.Plan.DurationDays);
            if (subscription.Rebate != null)
                _rebateService.IncreaseUseCount(
                    subscription.Rebate,
                    subscription.UserId,
                    decimal.ToDouble(subscription.RebatePrice));
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> MarkPaymentFailedAsync(long subscriptionId, long paymentId, CancellationToken cancellationToken = default)
        {
            var subscription = await _context.PastilAiSubscriptions.AsTracking()
                .FirstOrDefaultAsync(x => x.Id == subscriptionId, cancellationToken);
            if (subscription == null)
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }

            if (subscription.Status == PastilAiSubscriptionStatus.Active)
            {
                return new BaseResultDto(false, Resource.Notification.InvalidData);
            }

            subscription.PaymentId = paymentId;
            subscription.Status = PastilAiSubscriptionStatus.PaymentFailed;
            await _context.SaveChangesAsync(cancellationToken);
            return new BaseResultDto(true);
        }
    }
}
