using Entities.Entities.ShippingField;
using Microsoft.Extensions.Options;

namespace Application.Services.Order.ShippingSrv.Provider
{
    public class TipaxShippingProvider : TestModeShippingProviderBase
    {
        public TipaxShippingProvider(IOptions<ShippingOptions> options) : base(options) { }
        public override ShippingProviderEnum Provider => ShippingProviderEnum.Tipax;
        protected override double BasePrice => 120000;
        protected override double PricePerKilometer => 500;
        protected override double PricePerKilogram => 25000;
    }
}
