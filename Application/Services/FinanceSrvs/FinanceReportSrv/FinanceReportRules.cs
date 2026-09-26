using System.Collections.Generic;
using System.Linq;

namespace Application.Services.FinanceSrvs.FinanceReportSrv
{
    // قواعد محض گزارش مالی کل پاستیل: پول واردشده از هر منبع، مانده‌ی بازگشتی و سهم‌ها.
    public static class FinanceReportRules
    {
        // کلید منابع درآمد؛ همان نام PaymentCallbackTypeEnum (CompanionReserveBatch جزو CompanionReserve است)
        public static readonly string[] SourceKeys =
        {
            "ProductOrder", "CompanionReserve", "PansionReserve", "SchoolReserve",
            "ConsultationPurchase", "Trip", "Cargo", "Insurance", "PastilAI"
        };

        public static string NormalizeSource(string callBackTypeLabel)
            => callBackTypeLabel == "CompanionReserveBatch" ? "CompanionReserve" : callBackTypeLabel;

        // مبلغی که کاربر برای این منبع پرداخت کرده: درگاه + برداشت از کیف پول
        public static double Received(double gatewayPaid, double walletSpent) => gatewayPaid + walletSpent;

        // درآمد خالص = دریافتی منهای مبالغی که به کیف پول کاربر برگشته است
        public static double Net(double received, double refunded) => received - refunded;

        // سهم شریک (کلینیک/فروشگاه/راننده) = دریافتی - سهم سایت؛ هیچ‌وقت منفی نمی‌شود
        public static double PartnerShare(double received, double siteShare) => System.Math.Max(0, received - siteShare);

        public class SourceRow
        {
            public string Source { get; set; }
            public int PaymentCount { get; set; }
            public double GatewayPaid { get; set; }
            public double WalletSpent { get; set; }
            public double Received { get; set; }
            public double SiteShare { get; set; }
            public double PartnerShare { get; set; }
        }

        public class Summary
        {
            public List<SourceRow> Sources { get; set; } = new();
            public double TotalGatewayPaid { get; set; }
            public double TotalWalletSpent { get; set; }
            public double TotalReceived { get; set; }
            // مبلغ برگشت‌داده‌شده به کیف پول (اعتبارهای بدون پرداخت درگاهی: بازپرداخت/تعدیل)
            public double RefundedToWallet { get; set; }
            public double NetRevenue { get; set; }
            public double TotalSiteShare { get; set; }
            public double TotalPartnerShare { get; set; }
            // شارژ کیف پول از درگاه: درآمد نیست (بدهی سایت به کاربر) و جدا نمایش داده می‌شود
            public double WalletTopUps { get; set; }
        }

        public static Summary Build(IEnumerable<SourceRow> rows, double refundedToWallet, double walletTopUps)
        {
            var list = (rows ?? Enumerable.Empty<SourceRow>()).ToList();
            var summary = new Summary { Sources = list, RefundedToWallet = refundedToWallet, WalletTopUps = walletTopUps };
            summary.TotalGatewayPaid = list.Sum(x => x.GatewayPaid);
            summary.TotalWalletSpent = list.Sum(x => x.WalletSpent);
            summary.TotalReceived = list.Sum(x => x.Received);
            summary.TotalSiteShare = list.Sum(x => x.SiteShare);
            summary.TotalPartnerShare = list.Sum(x => x.PartnerShare);
            summary.NetRevenue = Net(summary.TotalReceived, refundedToWallet);
            return summary;
        }
    }
}
