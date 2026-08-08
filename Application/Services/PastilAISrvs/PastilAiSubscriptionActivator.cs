using Application.Common.Dto.Result;
using Application.Services.PastilAISrv.Iface;
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

        public PastilAiSubscriptionActivator(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto> ActivateAfterPaymentAsync(long subscriptionId, long paymentId, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var subscription = await _context.PastilAiSubscriptions.AsTracking()
                .Include(x => x.Plan).FirstOrDefaultAsync(x => x.Id == subscriptionId, cancellationToken);
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

            var now = DateTime.UtcNow;
            var currentEnd = await _context.PastilAiSubscriptions.AsNoTracking()
                .Where(x => x.UserId == subscription.UserId && x.Status == PastilAiSubscriptionStatus.Active && x.EndDateUtc > now)
                .MaxAsync(x => (DateTime?)x.EndDateUtc, cancellationToken);
            subscription.PaymentId = paymentId;
            subscription.Status = PastilAiSubscriptionStatus.Active;
            subscription.StartDateUtc = now;
            subscription.EndDateUtc = (currentEnd ?? now).AddDays(subscription.Plan.DurationDays);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new BaseResultDto(true);
        }
    }
}
