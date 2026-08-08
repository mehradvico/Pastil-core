using Application.Common.Enumerable.Code;
using Application.Services.Order.PaymentSrv;
using Xunit;

namespace Application.Tests.Payment
{
    public class PaymentAmountHelperTests
    {
        [Theory]
        [InlineData(50000, 40000, 40000)]
        [InlineData(5000, 20000, 5000)]
        [InlineData(15000, 20000, 10000)]
        [InlineData(5000, 9000, 0)]
        [InlineData(0, 50000, 0)]
        public void WalletContribution_PreservesGatewayMinimum(
            double balance,
            double payable,
            double expected)
        {
            Assert.Equal(expected, PaymentAmountHelper.GetWalletContribution(balance, payable));
        }

        [Fact]
        public void RebateScopes_AreDifferentForEveryPaymentMethod()
        {
            var scopes = new[]
            {
                RebateTypeLabels.Cart,
                RebateTypeLabels.CompanionReserve,
                RebateTypeLabels.Cargo,
                RebateTypeLabels.Trip,
                RebateTypeLabels.InsurancePackageSale,
                RebateTypeLabels.PansionReserve,
                RebateTypeLabels.PastilAI
            };

            Assert.Equal(scopes.Length, scopes.Distinct().Count());
        }
    }
}
