using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.PastilAISrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilAISrvs.PastilAiChat.Iface
{
    public interface IPastilAiChatService
    {
        Task<BaseResultDto<PastilAiAskResultDto>> AskAsync(long userId, PastilAiAskDto dto, CancellationToken cancellationToken);
        Task<PastilAiConversationSearchDto> GetUserConversationsAsync(long userId, BaseInputDto dto, CancellationToken cancellationToken);
        Task<BaseResultDto<PastilAiConversationDto>> GetUserConversationAsync(long userId, long id, CancellationToken cancellationToken);
        Task<PastilAiConversationSearchDto> SearchAdminAsync(PastilAiConversationInputDto dto, CancellationToken cancellationToken);
        Task<BaseResultDto<PastilAiConversationDto>> GetAdminConversationAsync(long id, CancellationToken cancellationToken);
    }
}
