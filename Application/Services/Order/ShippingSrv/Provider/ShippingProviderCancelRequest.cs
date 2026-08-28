using Entities.Entities.ShippingField;

namespace Application.Services.Order.ShippingSrv.Provider
{
    public class ShippingProviderCancelRequest
    {
        public ShippingProviderEnum Provider { get; set; }
        public string ExternalShipmentId { get; set; }
    }
}
