using Application.Common.Enumerable;
using Application.Services.Order.PaymentSrv;
using System;
using System.Globalization;

namespace Application.Services.ConsultationSrvs.ConsultationPurchaseSrv
{
    // قواعد خالص (بدون دیتابیس) خرید مشاوره: مبلغ‌ها، مجاز بودن گذارها، مهلت‌ها و کد خرید.
    // طراحی: backend/Docs/ONLINE_CONSULTATION_PACKAGES_FA.md §۴
    public static class ConsultationPurchaseRules
    {
        public readonly record struct Amounts(double Price, double RebatePrice, double Payable, double Wallet, double Gateway);

        // price = قیمت پکیج، rebatePrice = تخفیف پیشنهادی (سقف: کل قیمت)، walletBalance = موجودی قابل خرج (فقط نقد)
        public static Amounts ComputeAmounts(double price, double rebatePrice, bool fromWallet, double walletBalance)
        {
            price = Math.Max(0, price);
            var rebate = Math.Min(Math.Max(0, rebatePrice), price);
            var payable = Math.Max(0, price - rebate);
            var wallet = fromWallet ? PaymentAmountHelper.GetWalletContribution(walletBalance, payable) : 0;
            var gateway = Math.Max(0, payable - wallet);
            return new Amounts(price, rebate, payable, wallet, gateway);
        }

        public readonly record struct Shares(double Site, double Companion);

        // سهم سایت = مبلغ پرداخت‌شده (بعد از تخفیف) × درصد؛ باقی‌مانده سهم کلینیک. درصد بیرون ۰..۱۰۰ بریده می‌شود
        // و جمع دو سهم همیشه دقیقاً برابر مبلغ پرداخت‌شده است (مثل رزرو خدمت/پانسیون)
        public static Shares ComputeShares(double paymentPrice, decimal commissionPercent)
        {
            var total = Math.Max(0, paymentPrice);
            var percent = Math.Min(Math.Max(commissionPercent, 0m), 100m);
            var site = (double)((decimal)total * percent / 100m);
            return new Shares(site, total - site);
        }

        private static ConsultationPurchaseStatusEnum S(int status) => (ConsultationPurchaseStatusEnum)status;

        // لغو توسط کاربر فقط بعد از پرداخت و قبل از «شروع»
        public static bool CanUserCancel(int status) => S(status) == ConsultationPurchaseStatusEnum.Paid;

        // رزرو ساعت‌دار: لغو کاربر فقط تا ۲ ساعت قبل از ساعت رزرو؛ خرید فوری (scheduledStart = null) مثل قبل
        public static bool CanUserCancel(int status, DateTime? scheduledStart, DateTime now) =>
            CanUserCancel(status) && Application.Services.ConsultationSrvs.ConsultationBookingSrv.ConsultationBookingRules.IsUserCancelAllowed(scheduledStart, now);

        // شروع توسط نماینده فقط از «پرداخت‌شده» و تا مهلت
        // scheduledStart: رزرو ساعت‌دار؛ شروع فقط از ۱۰ دقیقه قبل از ساعت رزرو (null = خرید فوری، بدون محدودیت)
        public static bool CanStart(int status, DateTime? startDeadline, DateTime now, DateTime? scheduledStart) =>
            CanStart(status, startDeadline, now) && Application.Services.ConsultationSrvs.ConsultationBookingSrv.ConsultationBookingRules.IsStartAllowed(scheduledStart, now);

        public static bool CanStart(int status, DateTime? startDeadline, DateTime now) =>
            S(status) == ConsultationPurchaseStatusEnum.Paid && (!startDeadline.HasValue || now <= startDeadline.Value);

        // خریدِ پرداخت‌شده‌ای که نماینده تا مهلت شروع نکرده ⇒ لغو خودکار و بازپرداخت
        public static bool IsStartOverdue(int status, DateTime? startDeadline, DateTime now) =>
            S(status) == ConsultationPurchaseStatusEnum.Paid && startDeadline.HasValue && now > startDeadline.Value;

        // پنجره‌ی فعال: شروع شده و هنوز به ExpireDate نرسیده
        public static bool IsWindowOpen(int status, DateTime? expireDate, DateTime now) =>
            S(status) == ConsultationPurchaseStatusEnum.Active && expireDate.HasValue && now < expireDate.Value;

        // ورود دوباره‌ی نماینده: فقط در پنجره‌ی باز، و فقط نماینده‌ی شروع‌کننده یا مالک کلینیک
        public static bool CanAgentEnter(int status, DateTime? expireDate, DateTime now, long? purchaseAgentUserId, long userId, bool isOwner) =>
            IsWindowOpen(status, expireDate, now) && (purchaseAgentUserId == userId || isOwner);

        public static DateTime ComputeExpireDate(DateTime startDate, int durationMinutes) => startDate.AddMinutes(durationMinutes);

        public static DateTime ComputeStartDeadline(DateTime paidDate) => paidDate + ConsultationRules.StartDeadlineAfterPayment;

        // بازپرداخت = مبلغی که کاربر واقعاً پرداخته (بعد از تخفیف: هم درگاه هم کیف پول)؛ تخفیف قابل‌برگشت نیست
        public static double RefundAmount(double paymentPrice) => Math.Max(0, paymentPrice);

        // کد خرید خوانا و یکتا (یکتایی نهایی با ایندکس یکتای دیتابیس)
        public static string NewPurchaseCode(DateTime now, int random) =>
            string.Create(CultureInfo.InvariantCulture, $"CNS-{now:yyyyMMdd}-{now:HHmm}-{Math.Abs(random) % 10000:D4}");

        // نام ثبت بازپرداخت در دفتر کیف پول؛ برای جلوگیری از بازپرداخت دوباره (idempotency)
        public static string RefundLedgerName(long purchaseId) => $"ConsultationRefund:{purchaseId}";
    }
}
