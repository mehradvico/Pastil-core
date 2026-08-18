using Entities.Entities.ShippingField;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv.Provider
{
    public interface IShippingProvider
    {
        ShippingProviderEnum Provider { get; }
        Task<ShippingProviderQuoteResult> GetQuoteAsync(
            ShippingProviderQuoteRequest request,
            CancellationToken cancellationToken = default);
        Task<ShippingProviderShipmentResult> CreateShipmentAsync(
            ShippingProviderShipmentRequest request,
            CancellationToken cancellationToken = default);
    }
}
