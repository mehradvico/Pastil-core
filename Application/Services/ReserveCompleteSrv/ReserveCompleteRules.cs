using System;

namespace Application.Services.ReserveCompleteSrv
{
    /// <summary>
    /// بدنه‌ی اختیاری PUT /api/Companion/{Coach|Clinic|Groomer}Reserve/Complete?id=؛ بدون بدنه رفتار قبلی حفظ می‌شود.
    /// </summary>
    public class ReserveCompleteDto
    {
        /// <summary>مبلغ نهایی خدمت (≥ ۰ و ≥ مبلغ پرداخت آنلاین)؛ خالی = قیمت رزرو.</summary>
        public double? FinalAmount { get; set; }
        /// <summary>مبلغ دریافتی در محل (۰ تا FinalAmount − پرداخت آنلاین)؛ خالی = کل باقی‌مانده (یا ۰ اگر NotReceived).</summary>
        public double? ReceivedAtVenue { get; set; }
        /// <summary>true = باقی‌مانده‌ی دریافت‌نشده بدهی مشتری می‌شود.</summary>
        public bool NotReceived { get; set; }
        /// <summary>توضیح (حداکثر ۵۰۰ کاراکتر).</summary>
        public string Detail { get; set; }
    }

    public sealed record ReserveCompleteResult(string Error, double FinalAmount, double ReceivedAtVenue, bool NotReceived, double DebtAmount, string Detail);

    // طراحی و پاسخ سؤال‌های تیم اپ: backend/Docs/RESERVE_COMPLETE_FINAL_AMOUNT_NEMAYANDE_APP_FA.md
    public static class ReserveCompleteRules
    {
        public const int MaxDetailLength = 500;
        // تلورانس گرد کردن ریال (مثل PayFromWalletAsync)
        private const double Tolerance = 0.01;

        /// <summary>
        /// price = قیمت رزرو، paidOnline = سهم پرداخت آنلاین از قیمت (OnlineAmount؛ تخفیف هم جزو همین سهم است).
        /// بدهی = باقی‌مانده‌ی دریافت‌نشده فقط وقتی NotReceived؛ اگر NotReceived=false و دریافتی کمتر از باقی‌مانده بود، تفاوت را ارائه‌دهنده بخشیده است.
        /// </summary>
        public static ReserveCompleteResult Evaluate(double price, double paidOnline, ReserveCompleteDto dto)
        {
            dto ??= new ReserveCompleteDto();
            var detail = string.IsNullOrWhiteSpace(dto.Detail) ? null : dto.Detail.Trim();
            if (detail != null && detail.Length > MaxDetailLength)
                return Fail(Resource.Notification.ReserveCompleteDetailTooLong);

            var final = dto.FinalAmount ?? price;
            if (double.IsNaN(final) || double.IsInfinity(final) || final < 0)
                return Fail(Resource.Notification.ReserveCompleteFinalAmountInvalid);
            if (final + Tolerance < paidOnline)
                return Fail(Resource.Notification.ReserveCompleteFinalBelowOnline);

            var remainder = Math.Max(0, final - paidOnline);
            var received = dto.ReceivedAtVenue ?? (dto.NotReceived ? 0 : remainder);
            if (double.IsNaN(received) || double.IsInfinity(received) || received < 0 || received > remainder + Tolerance)
                return Fail(Resource.Notification.ReserveCompleteReceivedInvalid);
            received = Math.Min(received, remainder);

            var debt = dto.NotReceived ? Math.Round(remainder - received, 2) : 0;
            var notReceived = dto.NotReceived && debt > 0;
            return new ReserveCompleteResult(null, final, received, notReceived, notReceived ? debt : 0, detail);
        }

        /// <summary>فقط مالک یا پرسنلِ انتخاب‌شده‌ی همان رزرو؛ userId خالی = ادمین.</summary>
        public static bool CanComplete(long? userId, long ownerId, long? assigneeUserId)
            => !userId.HasValue || userId.Value == ownerId || (assigneeUserId.HasValue && assigneeUserId.Value == userId.Value);

        private static ReserveCompleteResult Fail(string error) => new(error, 0, 0, false, 0, null);
    }
}
