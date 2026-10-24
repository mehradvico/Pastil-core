using Entities.Entities.CommonField;
using System;

namespace Entities.Entities
{
    // بازه‌ی کاری هفتگی یک کلینیک برای مشاوره‌ی «قابل رزرو». کاربر فقط داخل همین بازه‌ها ساعت انتخاب می‌کند.
    // WeekDayId مثل CompanionAssistanceTime: ۱ شنبه … ۷ جمعه. ساعت‌ها به‌صورت دقیقه از نیمه‌شب (۰..۱۴۴۰) ذخیره می‌شوند.
    // Capacity = تعداد مشاوره‌ی هم‌زمان (معمولاً تعداد نمایندگان آماده در آن بازه). طراحی: backend/Docs/ONLINE_CONSULTATION_BOOKING_FA.md
    public class ConsultationAvailability : Id_Field
    {
        // مالک: دقیقاً یکی از سه ستون پر است (CHECK در دیتابیس). نمایندگی قدیمی = CompanionId؛ مراکز مستقل = ClinicId / CoachId
        public long? CompanionId { get; set; }
        public long? ClinicId { get; set; }
        public long? CoachId { get; set; }
        public int WeekDayId { get; set; }
        public int StartMinute { get; set; }
        public int EndMinute { get; set; }
        public int Capacity { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }

        public Companion Companion { get; set; }
        public ClinicField.Clinic Clinic { get; set; }
        public CoachField.Coach Coach { get; set; }
    }
}
