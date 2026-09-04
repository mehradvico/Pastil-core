using Entities.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv.Iface
{
    public interface IShipmentService
    {
        Task CreateForPaidOrderAsync(
            ProductOrder productOrder,
            CancellationToken cancellationToken = default);

        Task CancelForOrderAsync(
            string productOrderId,
            CancellationToken cancellationToken = default);

        Task HandleMiareWebhookAsync(
            string payload,
            CancellationToken cancellationToken = default);
    }
}
