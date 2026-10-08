using Application.Common.Enumerable.Code;
using System;

namespace Application.Services.PansionSrvs.PansionReserveSrv
{
    // قواعد خالص تأیید رزرو توسط مرکز بعد از پرداخت (بدون دیتابیس) تا تست‌پذیر باشد.
    public static class PansionReserveApprovalRules
    {
        /// <summary>مهلت پاسخ مرکز از لحظه‌ی پرداخت.</summary>
        public static readonly TimeSpan DecisionWindow = TimeSpan.FromHours(12);

        /// <summary>حداقل مهلت (حتی اگر شروع رزرو نزدیک باشد).</summary>
        public static readonly TimeSpan MinWindow = TimeSpan.FromMinutes(30);

        /// <summary>مهلت پاسخ باید دست‌کم این‌قدر قبل از شروع رزرو تمام شود.</summary>
        public static readonly TimeSpan StartBuffer = TimeSpan.FromHours(1);

        public static DateTime Deadline(DateTime now, DateTime? startAt)
        {
            var deadline = now + DecisionWindow;
            if (startAt.HasValue)
            {
                var beforeStart = startAt.Value - StartBuffer;
                if (beforeStart < deadline)
                    deadline = beforeStart;
            }
            var floor = now + MinWindow;
            return deadline < floor ? floor : deadline;
        }

        public static bool IsPending(int decision) => decision == (int)PansionReserveOwnerDecisionEnum.Pending;

        // فقط رزرو پرداخت‌شده، لغو‌نشده و منتظر پاسخ قابل تأیید/رد است
        public static bool CanDecide(bool isReserved, bool isCancel, int decision) =>
            isReserved && !isCancel && IsPending(decision);

        public static bool IsOverdue(int decision, DateTime? deadline, DateTime now) =>
            IsPending(decision) && deadline.HasValue && deadline.Value <= now;

        // «تکمیل‌شده» فقط برای رزروی که نیازی به تأیید ندارد یا تأیید شده است
        public static bool CanComplete(int decision) =>
            decision == (int)PansionReserveOwnerDecisionEnum.NotRequired ||
            decision == (int)PansionReserveOwnerDecisionEnum.Approved;

        public static string RefundLedgerName(long reserveId) => $"PansionReserveRefund:{reserveId}";
    }
}
