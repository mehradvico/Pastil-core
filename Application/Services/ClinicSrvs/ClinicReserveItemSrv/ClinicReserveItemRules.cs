using System;

namespace Application.Services.ClinicSrvs.ClinicReserveItemSrv
{
    /// <summary>وضعیت یک بسته داخل رزرو کلینیک (هم‌ارز CompanionReservePackageItemStatusEnum).</summary>
    public enum ClinicReserveItemStatusEnum
    {
        Pending = 1,   // در انتظار تأیید کلینیک
        Approved = 2,  // تأییدشده
        Cancelled = 3  // لغوشده؛ سهم پرداختی به کیف پول کاربر برگشته
    }

    /// <summary>قواعد خالص تأیید/لغو تکی بسته‌های رزرو کلینیک و محاسبه‌ی بازپرداخت.</summary>
    public static class ClinicReserveItemRules
    {
        public static bool CanTransition(int from, int to) =>
            (from == (int)ClinicReserveItemStatusEnum.Pending && to == (int)ClinicReserveItemStatusEnum.Approved)
            || (from == (int)ClinicReserveItemStatusEnum.Pending && to == (int)ClinicReserveItemStatusEnum.Cancelled)
            || (from == (int)ClinicReserveItemStatusEnum.Approved && to == (int)ClinicReserveItemStatusEnum.Cancelled);

        public static bool IsValidTarget(int status) =>
            status == (int)ClinicReserveItemStatusEnum.Approved || status == (int)ClinicReserveItemStatusEnum.Cancelled;

        /// <summary>تا وقتی رزرو پرداخت‌شده است و لغو/تکمیل نشده، بسته‌هایش قابل مدیریت‌اند.</summary>
        public static bool ReserveIsModifiable(bool isReserved, bool isCancel, int status, DateTime? doneDate, int completeStatus) =>
            isReserved && !isCancel && !doneDate.HasValue && status != completeStatus;

        /// <summary>
        /// سهم بازپرداخت یک بسته از مبلغِ باقی‌مانده‌ی پرداخت‌شده‌ی رزرو: تناسبی با وزن (سهم آنلاین بسته)؛ آخرین بسته‌ی فعال کل باقی‌مانده را
        /// می‌گیرد تا خطای گرد‌کردن نماند. به تومان گرد می‌شود و هرگز از باقی‌مانده بیشتر نیست.
        /// </summary>
        public static double RefundShare(double remainingPaid, double weight, double totalActiveWeight, bool isLastActive)
        {
            if (remainingPaid <= 0)
                return 0;
            if (isLastActive)
                return Math.Round(remainingPaid, 0);
            if (weight <= 0 || totalActiveWeight <= 0)
                return 0;
            var share = Math.Round(remainingPaid * weight / totalActiveWeight, 0);
            return Math.Min(Math.Max(share, 0), Math.Round(remainingPaid, 0));
        }

        public static string RefundLedgerName(long itemId) => $"ClinicPackageRefund:{itemId}";

        public static string PushKey(long itemId, int status) => $"clpkg:{itemId}:{status}";
    }
}
