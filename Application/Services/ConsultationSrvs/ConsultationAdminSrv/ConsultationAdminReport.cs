using Application.Common.Enumerable;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Dto;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.ConsultationSrvs.ConsultationAdminSrv
{
    // قواعد خالص (بدون دیتابیس) گزارش مالی خریدهای مشاوره؛ با تست واحد پوشش داده می‌شود.
    //
    // حسابداری:
    //   - «پرداخت‌شده» = Paid / Active / Completed  → مبلغ نزد پاستیل است و NetPaid حساب می‌شود.
    //   - Refunded → مبلغ به کیف پول کاربر برگشته (RefundedAmount)؛ در NetPaid نیست.
    //   - PendingPayment / Expired / Cancelled → هیچ پولی جابه‌جا نشده (یا هنوز بازپرداخت نشده)؛ در NetPaid نیست.
    //   - سهم کلینیک/پاستیل فقط برای خریدهای Completed قابل تسویه است (خدمت کامل ارائه شده و دیگر قابل بازپرداخت نیست).
    public static class ConsultationAdminReport
    {
        public readonly record struct StatusGroup(
            int Status,
            bool Permitted,
            int Count,
            double Price,
            double Rebate,
            double Paid,
            double CompanionShare,
            double SiteShare,
            int Minutes);

        public static bool IsPaidStatus(int status) =>
            status == (int)ConsultationPurchaseStatusEnum.Paid ||
            status == (int)ConsultationPurchaseStatusEnum.Active ||
            status == (int)ConsultationPurchaseStatusEnum.Completed;

        public static double NetPaid(int status, double paymentPrice) => IsPaidStatus(status) ? paymentPrice : 0;

        public static double RefundedAmount(int status, double paymentPrice) =>
            status == (int)ConsultationPurchaseStatusEnum.Refunded ? paymentPrice : 0;

        public static ConsultationPurchaseAdminSummaryDto BuildSummary(IEnumerable<StatusGroup> groups)
        {
            var list = (groups ?? Enumerable.Empty<StatusGroup>()).ToList();
            var summary = new ConsultationPurchaseAdminSummaryDto
            {
                TotalCount = list.Sum(g => g.Count)
            };

            foreach (var byStatus in list.GroupBy(g => g.Status))
                summary.CountByStatus[byStatus.Key] = byStatus.Sum(g => g.Count);

            var paid = list.Where(g => IsPaidStatus(g.Status)).ToList();
            summary.GrossPrice = paid.Sum(g => g.Price);
            summary.RebateTotal = paid.Sum(g => g.Rebate);
            summary.NetPaidTotal = paid.Sum(g => g.Paid);
            summary.RefundedTotal = list.Where(g => g.Status == (int)ConsultationPurchaseStatusEnum.Refunded).Sum(g => g.Paid);

            var completed = list.Where(g => g.Status == (int)ConsultationPurchaseStatusEnum.Completed).ToList();
            summary.CompanionShareCompleted = completed.Sum(g => g.CompanionShare);
            summary.SiteShareCompleted = completed.Sum(g => g.SiteShare);
            summary.SettledShare = completed.Where(g => g.Permitted).Sum(g => g.CompanionShare);
            summary.UnsettledShare = completed.Where(g => !g.Permitted).Sum(g => g.CompanionShare);

            summary.PurchasedMinutes = list
                .Where(g => g.Status == (int)ConsultationPurchaseStatusEnum.Active || g.Status == (int)ConsultationPurchaseStatusEnum.Completed)
                .Sum(g => g.Minutes);

            return summary;
        }
    }
}
