using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilClubSrvs.PointEventSrv.Iface
{
    public interface IClubPointIntegrationService
    {
        Task ProductOrderCompletedAsync(long userId, string orderId, CancellationToken cancellationToken = default);
        Task ProductOrderReversedAsync(long userId, string orderId, CancellationToken cancellationToken = default);
        Task CompanionReserveCompletedAsync(long userId, long reserveId, CancellationToken cancellationToken = default);
        Task CompanionReserveReversedAsync(long userId, long reserveId, CancellationToken cancellationToken = default);
        Task PansionReserveCompletedAsync(long userId, long reserveId, CancellationToken cancellationToken = default);
        Task PansionReserveReversedAsync(long userId, long reserveId, CancellationToken cancellationToken = default);
        Task PetProfileCompletedAsync(long userId, long userPetId, CancellationToken cancellationToken = default);
        Task MemoryCreatedAsync(long userId, long memoryId, DateTimeOffset memoryDate, CancellationToken cancellationToken = default);
        Task MemoryReversedAsync(long userId, long memoryId, DateTimeOffset memoryDate, CancellationToken cancellationToken = default);
    }
}
