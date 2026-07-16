using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.TicketSrv.Dto;
using Entities.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Accounting.TicketSrv.Iface
{
    public interface ITicketService
    {
        Task<TicketSearchDto> SearchUserAsync(TicketInputDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<TicketVDto>> FindCurrentAdminAsync(long id, CancellationToken cancellationToken = default);
        Task<TicketSearchDto> SearchAdminAsync(TicketInputDto dto, CancellationToken cancellationToken = default);
        Task<TicketSearchDto> SearchCurrentAdminAsync(TicketInputDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<TicketVDto>> FindUserAsync(long id, CancellationToken cancellationToken = default);
        Task<BaseResultDto<TicketVDto>> FindAdminAsync(long id, CancellationToken cancellationToken = default);
        Task<BaseResultDto<TicketVDto>> InsertUserAsyncDto(CreateTicketDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<TicketVDto>> InsertAdminAsyncDto(CreateAdminTicketDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto> ChangeStatusAsync(ChangeTicketStatusDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto> ChangeImportanceAsync(ChangeTicketImportanceDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto> AssignAdminAsync(AssignTicketAdminDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto> DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task CloseTicketAsync(int hours = 24, CancellationToken cancellationToken = default);
    }
}
