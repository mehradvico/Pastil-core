using System;

namespace Entities.Entities.CommonField
{
    /// <summary>
    /// رزروهای مربی/کلینیک/آرایشگاه که ارائه‌دهنده موقع «تکمیل» مبلغ نهایی و دریافتی در محل را ثبت می‌کند.
    /// بدهی باز = NotReceived && DebtAmount &gt; 0 && DebtPaidDate == null && !IsCancel؛ تاریخ شروع مهلت = DoneDate.
    /// طراحی: backend/Docs/RESERVE_COMPLETE_FINAL_AMOUNT_NEMAYANDE_APP_FA.md
    /// </summary>
    public interface IVenueCompletableReserve
    {
        long Id { get; set; }
        string ReserveCode { get; set; }
        long BookerId { get; set; }
        bool IsCancel { get; set; }
        bool Permitted { get; set; }
        decimal CommissionPercent { get; set; }
        double ProviderShare { get; set; }
        double SiteShare { get; set; }
        DateTime? DoneDate { get; set; }

        /// <summary>مبلغ نهایی خدمت که ارائه‌دهنده ثبت کرد؛ null = تکمیل قدیمی/بدون بدنه (همان Price).</summary>
        double? FinalAmount { get; set; }
        /// <summary>مبلغی که ارائه‌دهنده در محل از مشتری گرفت (خارج از حسابداری پاستیل).</summary>
        double ReceivedAtVenue { get; set; }
        /// <summary>true = باقی‌مانده دریافت نشد و بدهی مشتری شد.</summary>
        bool NotReceived { get; set; }
        string CompleteDetail { get; set; }
        long? CompletedByUserId { get; set; }
        /// <summary>مبلغ بدهی مشتری (FinalAmount − OnlineAmount − ReceivedAtVenue وقتی NotReceived).</summary>
        double DebtAmount { get; set; }
        DateTime? DebtPaidDate { get; set; }
        bool DebtPaidByWallet { get; set; }
    }
}
