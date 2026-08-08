using System;

namespace Application.Services.Order.PaymentSrv
{
    public static class PaymentAmountHelper
    {
        public const double MinimumGatewayAmount = 10000;

        public static double GetWalletContribution(double walletBalance, double payableAmount)
        {
            if (walletBalance <= 0 || payableAmount <= 0)
                return 0;
            if (walletBalance >= payableAmount)
                return payableAmount;

            var maximumContribution = payableAmount - MinimumGatewayAmount;
            return maximumContribution > 0 ? Math.Min(walletBalance, maximumContribution) : 0;
        }
    }
}
