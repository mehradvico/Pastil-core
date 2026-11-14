using Application.Common.Enumerable;
using System;

namespace Application.Services.MarketingSrv
{
    // قواعد خالص یادآوری «پرداخت ناموفق» و «رزرو پرداخت‌نشده» (سند اتوماسیون پیام، سفر ۲؛ تأیید ۱۴۰۵/۰۸/۲۳).
    //   پرداخت ناموفق (همه‌ی انواع): پوش ۵ دقیقه بعد (اگر درگاه اصلاً برنگشت ۲۰ دقیقه بعد)، پیامک ۱ ساعت بعد از پوش.
    //   رزرو پرداخت‌نشده (همه‌ی انواع): پوش ۳۰ دقیقه بعد از ثبت، پیامک ۳ ساعت بعد از ثبت (و دست‌کم ۲ ساعت بعد از پوش).
    // هر پرداخت موفق کاربر بعد از آن تلاش، یادآوری را قطع می‌کند. ساعت سکوت در MarketingQuietHours است.
    public static class PaymentReminderRules
    {
        public static readonly TimeSpan FailedPushDelay = TimeSpan.FromMinutes(5);
        /// <summary>تلاشی که درگاه هنوز جوابش را نداده (IsSuccess = null) شاید کاربر هنوز در صفحه‌ی بانک باشد.</summary>
        public static readonly TimeSpan AbandonedPushDelay = TimeSpan.FromMinutes(20);
        public static readonly TimeSpan FailedSmsAfterPush = TimeSpan.FromHours(1);
        public static readonly TimeSpan FailedMaxAge = TimeSpan.FromHours(24);

        public static readonly TimeSpan UnpaidPushDelay = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan UnpaidSmsDelay = TimeSpan.FromHours(3);
        public static readonly TimeSpan UnpaidSmsAfterPush = TimeSpan.FromHours(2);
        public static readonly TimeSpan UnpaidMaxAge = TimeSpan.FromHours(24);

        /// <summary>هر نوع پیامک یادآوری حداکثر یک‌بار در این بازه به یک شماره می‌رود.</summary>
        public static readonly TimeSpan SmsCooldown = TimeSpan.FromHours(24);

        public static bool IsFailedPushDue(bool? isSuccess, DateTime attemptAt, DateTime now)
        {
            if (isSuccess == true)
                return false;
            var age = now - attemptAt;
            var delay = isSuccess == false ? FailedPushDelay : AbandonedPushDelay;
            return age >= delay && age <= FailedMaxAge;
        }

        public static bool IsFailedSmsDue(DateTime attemptAt, DateTime pushAt, DateTime now) =>
            now - attemptAt <= FailedMaxAge && now - pushAt >= FailedSmsAfterPush;

        public static bool IsUnpaidPushDue(DateTime createdAt, DateTime now)
        {
            var age = now - createdAt;
            return age >= UnpaidPushDelay && age <= UnpaidMaxAge;
        }

        public static bool IsUnpaidSmsDue(DateTime createdAt, DateTime pushAt, DateTime now)
        {
            var age = now - createdAt;
            return age >= UnpaidSmsDelay && age <= UnpaidMaxAge && now - pushAt >= UnpaidSmsAfterPush;
        }

        // صفحه‌ی وب‌اپ برای پرداخت دوباره، بر اساس نوع پرداخت (Payment.CallBackTypeLabel)
        public static string RetryPath(string callBackTypeLabel, string callBackId)
        {
            if (!Enum.TryParse<PaymentCallbackTypeEnum>(callBackTypeLabel, out var type))
                return "/";
            return type switch
            {
                PaymentCallbackTypeEnum.ProductOrder => "/orders",
                PaymentCallbackTypeEnum.Trip => string.IsNullOrWhiteSpace(callBackId) ? "/transportation" : "/trip/" + callBackId,
                PaymentCallbackTypeEnum.Wallet => "/wallet",
                PaymentCallbackTypeEnum.PastilAI => "/ai",
                PaymentCallbackTypeEnum.ConsultationPurchase => "/consultations",
                PaymentCallbackTypeEnum.CompanionReserve or PaymentCallbackTypeEnum.CompanionReserveBatch or
                PaymentCallbackTypeEnum.PansionReserve or PaymentCallbackTypeEnum.SchoolReserve or
                PaymentCallbackTypeEnum.GroomerReserve or PaymentCallbackTypeEnum.ClinicReserve or
                PaymentCallbackTypeEnum.CoachReserve => "/reserve",
                _ => "/"
            };
        }
    }
}
