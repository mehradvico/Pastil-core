using Entities.Entities.CommonField;
using System;

namespace Entities.Entities
{
    // یک راننده که یک سفرِ مشخص را رد کرده یا (پس از پذیرفتن) لغو کرده — این سفر دیگر برای همان راننده
    // نمایش داده نمی‌شود. هنگام ثبت درخواست جدید بعد از لغوِ راننده، این ردیف‌ها به سفر جدید هم منتقل می‌شوند.
    public class TripDriverExclusion : Id_Field
    {
        public long TripId { get; set; }
        public long DriverId { get; set; }
        public int ReasonId { get; set; }
        public DateTime CreateDate { get; set; }

        public Trip Trip { get; set; }
        public Driver Driver { get; set; }
    }
}
