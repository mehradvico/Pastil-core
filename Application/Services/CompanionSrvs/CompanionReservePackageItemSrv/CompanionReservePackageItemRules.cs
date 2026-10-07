using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using System;
using System.Collections.Generic;

namespace Application.Services.CompanionSrvs.CompanionReservePackageItemSrv
{
    // قواعد خالصِ تأیید/لغو تکی پکیج‌های رزرو و محاسبه‌ی بازپرداخت. طراحی: backend/Docs/COMPANION_RESERVE_PACKAGE_ITEMS_FA.md
    public static class CompanionReservePackageItemRules
    {
        public static bool CanTransition(int from, int to)
        {
            return (from == (int)CompanionReservePackageItemStatusEnum.Pending && to == (int)CompanionReservePackageItemStatusEnum.Approved)
                || (from == (int)CompanionReservePackageItemStatusEnum.Pending && to == (int)CompanionReservePackageItemStatusEnum.Cancelled)
                || (from == (int)CompanionReservePackageItemStatusEnum.Approved && to == (int)CompanionReservePackageItemStatusEnum.Cancelled);
        }

        public static bool IsValidTarget(int status) =>
            status == (int)CompanionReservePackageItemStatusEnum.Approved || status == (int)CompanionReservePackageItemStatusEnum.Cancelled;

        /// <summary>
        /// تا وقتی رزرو لغو/کامل/«نهایی‌شده» نشده و هنوز خدمت انجام نشده، پکیج‌هایش قابل مدیریت‌اند.
        /// stateId: کد وضعیت رزرو (۳۰ ثبت‌شده، ۳۱ پیش‌پرداخت‌شده، ۳۲ پرداخت‌شده/نهایی، ۳۳ کامل).
        /// </summary>
        public static bool ReserveIsModifiable(bool isCancel, long stateId, long operatorStateId, DateTime? doneDate)
        {
            if (isCancel || doneDate.HasValue)
                return false;
            if (operatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Complete
                || operatorStateId == (long)CompanionReserveOperatorStateEnum.OperatorState_Cancelled)
                return false;
            return stateId == (long)CompanionReserveStateEnum.CompanianReserveState_Registered
                || stateId == (long)CompanionReserveStateEnum.CompanianReserveState_PrePaid;
        }

        /// <summary>
        /// سهم بازپرداخت یک پکیج از مبلغِ باقی‌مانده‌ی پرداخت‌شده‌ی رزرو. تناسبی با وزن پیش‌پرداخت؛
        /// اگر آخرین پکیج فعال باشد (یا وزن‌ها صفر باشد و فقط یکی بماند) کل باقی‌مانده برمی‌گردد تا خطای گرد‌کردن نماند.
        /// نتیجه به تومان کامل گرد می‌شود و هرگز از باقی‌مانده بیشتر نیست.
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

        public static string RefundLedgerName(long itemId) => $"CompanionPackageRefund:{itemId}";

        public static string PushKey(long itemId, int status) => $"crpkg:{itemId}:{status}";
    }
}
