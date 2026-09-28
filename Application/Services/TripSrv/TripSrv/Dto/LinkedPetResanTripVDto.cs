namespace Application.Services.TripSrv.TripSrv.Dto
{
    // خلاصه‌ی سفر پت‌رسانِ متصل به یک رزرو خدمت/پانسیون/مدرسه (Trip.CompanionReserveId/PansionReserveId/SchoolReserveId).
    // پرداختش کاملاً جدا از رزرو است (مثل هر سفر آنلاین دیگر: مبلغ فقط بعد از قبول راننده از کاربر کسر می‌شود)؛
    // این فقط برای نمایش «این رزرو یک سفر پت‌رسان هم دارد» و مبلغش در پنل/گزارش مالی است.
    public class LinkedPetResanTripVDto
    {
        public long TripId { get; set; }
        public double Price { get; set; }
        public double PaymentPrice { get; set; }
        public bool IsPaid { get; set; }
        public string TripStatusLabel { get; set; }
        public string DriverStatusLabel { get; set; }
        public long? DriverId { get; set; }
        public string DriverName { get; set; }
    }
}
