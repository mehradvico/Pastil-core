using Entities.Entities.ShippingField;
using Microsoft.Extensions.Options;

namespace Application.Services.Order.ShippingSrv.Provider
{
    public class AloPeykShippingProvider : TestModeShippingProviderBase
    {
        public AloPeykShippingProvider(IOptions<ShippingOptions> options) : base(options) { }
        public override ShippingProviderEnum Provider => ShippingProviderEnum.AloPeyk;
        protected override double BasePrice => 60000;
        protected override double PricePerKilometer => 10000;
    }
}
