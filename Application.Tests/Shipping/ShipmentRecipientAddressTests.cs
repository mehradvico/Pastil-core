using Application.Services.Order.ShippingSrv;
using Entities.Entities;
using Xunit;

namespace Application.Tests.Shipping
{
    public class ShipmentRecipientAddressTests
    {
        [Fact]
        public void ComposeRecipientAddress_AppendsFloorAndUnit_WhenPresent()
        {
            var text = ShipmentService.ComposeRecipientAddress(new Address
            {
                AddressValue = "تهران، خیابان ولیعصر",
                Floor = "۳",
                Unit = " ۱۲ "
            });

            Assert.Equal($"تهران، خیابان ولیعصر، {Resource.Field.Floor} ۳، {Resource.Field.Unit} ۱۲", text);
        }

        [Fact]
        public void ComposeRecipientAddress_SkipsEmptyFloorAndUnit()
        {
            var text = ShipmentService.ComposeRecipientAddress(new Address
            {
                AddressValue = "تهران، خیابان ولیعصر",
                Floor = " ",
                Unit = null
            });

            Assert.Equal("تهران، خیابان ولیعصر", text);
        }

        [Fact]
        public void ComposeRecipientAddress_ReturnsNull_WhenThereIsNoAddress()
        {
            Assert.Null(ShipmentService.ComposeRecipientAddress(null));
        }
    }
}
