using Application.Common.Dto.Result;
using Application.Services.Accounting.TicketItemSrv.Dto;
using Application.Services.Accounting.TicketMessageSrv.Dto;
using Entities.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Accounting.TicketMessageSrv.Iface
{
    public interface ITicketMessageService
    {
        Task<BaseResultDto<TicketMessageSearchDto>> GetUserMessagesAsync(long ticketId, TicketMessageInputDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<TicketMessageSearchDto>> GetAdminMessagesAsync(long ticketId, TicketMessageInputDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<TicketItemVDto>> SendUserMessageAsync(long ticketId, SendTicketMessageDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<TicketItemVDto>> SendAdminMessageAsync(long ticketId, SendTicketMessageDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<TicketSeenVDto>> MarkAsSeenForUserAsync(long ticketId, CancellationToken cancellationToken = default);
        Task<BaseResultDto<TicketSeenVDto>> MarkAsSeenForAdminAsync(long ticketId, CancellationToken cancellationToken = default);
        Task<BaseResultDto<TicketItem>> PrepareInitialMessageAsync(Ticket ticket, long senderUserId, string body, long? fileId, CancellationToken cancellationToken = default);
    }
}