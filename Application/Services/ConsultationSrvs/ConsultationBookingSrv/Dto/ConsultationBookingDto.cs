using System;
using System.Collections.Generic;

namespace Application.Services.ConsultationSrvs.ConsultationBookingSrv.Dto
{
    // یک بازه‌ی کاری هفتگی کلینیک برای مشاوره‌ی قابل‌رزرو. ساعت‌ها «HH:mm» (مثلاً "16:00")، پایان «24:00» هم مجاز است.
    public class ConsultationAvailabilityWindowDto
    {
        // ۱ شنبه، ۲ یکشنبه، ۳ دوشنبه، ۴ سه‌شنبه، ۵ چهارشنبه، ۶ پنجشنبه، ۷ جمعه
        public int WeekDayId { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        // تعداد مشاوره‌ی هم‌زمان در این بازه (۱ تا ۲۰)
        public int Capacity { get; set; } = 1;
    }

    public class ConsultationSlotVDto
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        // false = ظرفیت پر است (برای نمایش خاکستری؛ قابل خرید نیست)
        public bool Available { get; set; }
    }

    public class ConsultationSlotsVDto
    {
        public long PackageId { get; set; }
        public int DurationMinutes { get; set; }
        // روز درخواست‌شده (۰۰:۰۰)
        public DateTime Date { get; set; }
        public List<ConsultationSlotVDto> Slots { get; set; } = new();
        // ساعت سرور؛ مبنای محاسبه‌ی «حداقل فاصله تا الان» (کلاینت از ساعت دستگاه استفاده نکند)
        public DateTime ServerNow { get; set; }
    }

    public class ConsultationBookingDayVDto
    {
        public DateTime Date { get; set; }
        public int WeekDayId { get; set; }
        public int AvailableSlots { get; set; }
    }

    public class ConsultationBookingDaysVDto
    {
        public long PackageId { get; set; }
        public int DurationMinutes { get; set; }
        public List<ConsultationBookingDayVDto> Days { get; set; } = new();
        public DateTime ServerNow { get; set; }
    }
}
