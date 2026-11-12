namespace Application.Services.TripSrv.TripSrv.Dto
{
    // خلاصه‌ی سفر پت‌رسانِ متصل به یک رزرو خدمت/پانسیون/مدرسه (Trip.CompanionReserveId/PansionReserveId/SchoolReserveId).
    // PaidWithReserve = true: مبلغ سفر همراه خودِ رزرو (یک پرداخت) گرفته شده/می‌شود؛ false: پرداخت جدا بعد از قبول راننده.
    // مبلغ‌ها فقط برای مشتری و ادمین است؛ برای مرکز (مالک/پرسنل) HideAmounts صفرشان می‌کند.
    public class LinkedPetResanTripVDto
    {
        public long TripId { get; set; }
        public double Price { get; set; }
        public double PaymentPrice { get; set; }
        public bool IsPaid { get; set; }
        public bool PaidWithReserve { get; set; }
        /// <summary>مبلغ‌ها برای این بیننده (مرکز) پنهان شده‌اند.</summary>
        public bool AmountsHidden { get; set; }
        /// <summary>زمان حرکت راننده (زمان نوبت منهای ۶۰ یا ۱۲۰ دقیقه).</summary>
        public System.DateTime? ScheduledDepartureAt { get; set; }
        public int? ScheduledLeadMinutes { get; set; }
        public string TripStatusLabel { get; set; }
        public string DriverStatusLabel { get; set; }
        public long? DriverId { get; set; }
        public string DriverName { get; set; }

        /// <summary>مبلغ قابل پرداخت سفر (بعد از تخفیف).</summary>
        public double Payable() => PaymentPrice > 0 ? PaymentPrice : Price;

        /// <summary>مرکز فقط مبلغ رزرو خودش را می‌بیند؛ مبلغ پت‌رسان صفر می‌شود.</summary>
        public LinkedPetResanTripVDto HideAmounts()
        {
            Price = 0;
            PaymentPrice = 0;
            AmountsHidden = true;
            return this;
        }
    }
}
