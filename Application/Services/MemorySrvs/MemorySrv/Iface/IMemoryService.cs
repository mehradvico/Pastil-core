using Application.Common.Dto.Result;
using Application.Services.MemorySrvs.MemorySrv.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.MemorySrvs.MemorySrv.Iface
{
    public interface IMemoryService
    {
        Task<BaseResultDto<MemoryVDto>> FindAsync(long id, long? userId, CancellationToken cancellationToken = default);
        Task<MemorySearchDto> SearchAsync(MemoryInputDto dto, long? userId, CancellationToken cancellationToken = default);
        Task<BaseResultDto<MemoryVDto>> InsertAsync(long userId, MemoryDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto<MemoryVDto>> UpdateAsync(long userId, MemoryDto dto, CancellationToken cancellationToken = default);
        Task<BaseResultDto> DeleteAsync(long userId, long id, CancellationToken cancellationToken = default);
        Task SendDailyReminderAsync(CancellationToken cancellationToken = default);
    }
}
