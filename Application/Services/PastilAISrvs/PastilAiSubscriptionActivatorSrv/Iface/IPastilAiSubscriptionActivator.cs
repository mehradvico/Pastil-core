using Application.Common.Dto.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilAISrvs.PastilAiSubscriptionActivatorSrv.Iface
{
    public interface IPastilAiSubscriptionActivator
    {
        Task<BaseResultDto> ActivateAfterPaymentAsync(long subscriptionId, long paymentId, CancellationToken cancellationToken = default);
        Task<BaseResultDto> MarkPaymentFailedAsync(long subscriptionId, long paymentId, CancellationToken cancellationToken = default);
    }
}
