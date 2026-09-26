using Application.Common.Enumerable;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv;
using System.Collections.Generic;
using Xunit;
using G = Application.Services.ConsultationSrvs.ConsultationAdminSrv.ConsultationAdminReport.StatusGroup;

namespace Application.Tests;

// حسابداری گزارش خریدهای مشاوره: فقط پرداخت‌شده‌ها پول حساب می‌شوند، بازپرداخت‌شده‌ها جدا، و سهم فقط برای «تکمیل‌شده».
public class ConsultationAdminReportTests
{
    private const int Pending = (int)ConsultationPurchaseStatusEnum.PendingPayment;
    private const int Paid = (int)ConsultationPurchaseStatusEnum.Paid;
    private const int Active = (int)ConsultationPurchaseStatusEnum.Active;
    private const int Completed = (int)ConsultationPurchaseStatusEnum.Completed;
    private const int Expired = (int)ConsultationPurchaseStatusEnum.Expired;
    private const int Cancelled = (int)ConsultationPurchaseStatusEnum.Cancelled;
    private const int Refunded = (int)ConsultationPurchaseStatusEnum.Refunded;

    [Theory]
    [InlineData(Paid, 100, 100)]
    [InlineData(Active, 100, 100)]
    [InlineData(Completed, 100, 100)]
    [InlineData(Pending, 100, 0)]
    [InlineData(Expired, 100, 0)]
    [InlineData(Cancelled, 100, 0)]
    [InlineData(Refunded, 100, 0)]
    public void Net_paid_counts_only_paid_active_and_completed(int status, double payment, double expected)
    {
        Assert.Equal(expected, ConsultationAdminReport.NetPaid(status, payment));
    }

    [Theory]
    [InlineData(Refunded, 250, 250)]
    [InlineData(Completed, 250, 0)]
    [InlineData(Cancelled, 250, 0)]
    public void Refunded_amount_only_for_refunded_purchases(int status, double payment, double expected)
    {
        Assert.Equal(expected, ConsultationAdminReport.RefundedAmount(status, payment));
    }

    [Fact]
    public void Summary_of_nothing_is_all_zero()
    {
        var summary = ConsultationAdminReport.BuildSummary(null);
        Assert.Equal(0, summary.TotalCount);
        Assert.Empty(summary.CountByStatus);
        Assert.Equal(0, summary.NetPaidTotal + summary.RefundedTotal + summary.CompanionShareCompleted + summary.SettledShare + summary.UnsettledShare);
    }

    [Fact]
    public void Summary_separates_revenue_refunds_and_settlement()
    {
        var groups = new List<ConsultationAdminReport.StatusGroup>
        {
            //          status     permitted count price  rebate paid  compShare siteShare minutes
            new(Pending,   false, 2, 400_000, 0,      400_000, 0,       0, 60),      // پرداخت نشده: هیچ‌جا حساب نمی‌شود
            new(Paid,      false, 1, 200_000, 0,      200_000, 200_000, 0, 30),
            new(Active,    false, 1, 300_000, 30_000, 270_000, 270_000, 0, 60),
            new(Completed, false, 3, 600_000, 60_000, 540_000, 540_000, 0, 90),      // تسویه‌نشده
            new(Completed, true,  2, 400_000, 0,      400_000, 400_000, 0, 60),      // تسویه‌شده
            new(Refunded,  false, 1, 150_000, 0,      150_000, 0,       0, 30),
            new(Expired,   false, 1, 100_000, 0,      100_000, 0,       0, 15),
        };

        var s = ConsultationAdminReport.BuildSummary(groups);

        Assert.Equal(11, s.TotalCount);
        Assert.Equal(2, s.CountByStatus[Pending]);
        Assert.Equal(5, s.CountByStatus[Completed]);

        // فقط Paid + Active + Completed
        Assert.Equal(200_000 + 300_000 + 600_000 + 400_000, s.GrossPrice);
        Assert.Equal(30_000 + 60_000, s.RebateTotal);
        Assert.Equal(200_000 + 270_000 + 540_000 + 400_000, s.NetPaidTotal);
        Assert.Equal(150_000, s.RefundedTotal);

        // سهم فقط برای تکمیل‌شده‌ها؛ تسویه‌شده و تسویه‌نشده جدا
        Assert.Equal(540_000 + 400_000, s.CompanionShareCompleted);
        Assert.Equal(400_000, s.SettledShare);
        Assert.Equal(540_000, s.UnsettledShare);
        Assert.Equal(s.CompanionShareCompleted, s.SettledShare + s.UnsettledShare);

        // مدت خریداری‌شده فقط فعال + تکمیل‌شده
        Assert.Equal(60 + 90 + 60, s.PurchasedMinutes);
    }

    [Fact]
    public void Refunded_purchase_never_adds_to_revenue()
    {
        var s = ConsultationAdminReport.BuildSummary(new[] { new G(Refunded, false, 1, 100_000, 0, 100_000, 0, 0, 30) });
        Assert.Equal(0, s.NetPaidTotal);
        Assert.Equal(0, s.GrossPrice);
        Assert.Equal(100_000, s.RefundedTotal);
    }
}
