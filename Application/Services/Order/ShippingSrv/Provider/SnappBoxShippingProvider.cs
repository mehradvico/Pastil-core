using Entities.Entities.ShippingField;
using Microsoft.Extensions.Options;

namespace Application.Services.Order.ShippingSrv.Provider
{
    public class SnappBoxShippingProvider : TestModeShippingProviderBase
    {
        public SnappBoxShippingProvider(IOptions<ShippingOptions> options) : base(options) { }
        public override ShippingProviderEnum Provider => ShippingProviderEnum.SnappBox;
        protected override double BasePrice => 55000;
        protected override double PricePerKilometer => 9000;
    }
}
