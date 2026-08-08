using Application.Common.Dto.Result;
using Application.Common.Dto.Input;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.PastilAISrv.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilAISrv.Iface
{
    public interface IPastilAiPlanService
    {
        Task<BaseResultDto<List<PastilAiPlanVDto>>> GetPlansAsync(bool admin, CancellationToken cancellationToken);
        Task<BaseResultDto<PastilAiPlanVDto>> UpdateAsync(PastilAiPlanUpdateDto dto, CancellationToken cancellationToken);
        Task<BaseResultDto<PastilAiQuotaDto>> GetQuotaAsync(long userId, CancellationToken cancellationToken);
        Task<BaseResultDto> PurchaseAsync(long userId, PastilAiPurchaseDto dto, CancellationToken cancellationToken);
    }

    public interface IPastilAiSubscriptionActivator
    {
        Task<BaseResultDto> ActivateAfterPaymentAsync(long subscriptionId, long paymentId, CancellationToken cancellationToken = default);
        Task<BaseResultDto> MarkPaymentFailedAsync(long subscriptionId, long paymentId, CancellationToken cancellationToken = default);
    }

    public interface IPastilAiChatService
    {
        Task<BaseResultDto<PastilAiAskResultDto>> AskAsync(long userId, PastilAiAskDto dto, CancellationToken cancellationToken);
        Task<PastilAiConversationSearchDto> GetUserConversationsAsync(long userId, BaseInputDto dto, CancellationToken cancellationToken);
        Task<BaseResultDto<PastilAiConversationDto>> GetUserConversationAsync(long userId, long id, CancellationToken cancellationToken);
        Task<PastilAiConversationSearchDto> SearchAdminAsync(PastilAiConversationInputDto dto, CancellationToken cancellationToken);
        Task<BaseResultDto<PastilAiConversationDto>> GetAdminConversationAsync(long id, CancellationToken cancellationToken);
    }
}
