using Entities.Entities.CommonField;
using System;

namespace Entities.Entities
{
    // پکیج «مشاوره آنلاین» یک کلینیک: کانال × مدت × قیمت. کاملاً جدا از CompanionAssistancePackage است
    // (طراحی: backend/Docs/ONLINE_CONSULTATION_PACKAGES_FA.md). هر کلینیک حداکثر یک ردیف زنده برای هر (کانال، مدت) دارد.
    public class ConsultationPackage : Id_Field
    {
        public long CompanionId { get; set; }
        // مقدار OnlineSessionChannelEnum (۱ چت، ۲ تماس درون‌برنامه، ۳ تماس تصویری، ۴ تماس تلفنی)
        public int ChannelId { get; set; }
        // ۳۰ یا ۶۰ (ConsultationRules.AllowedDurations)
        public int DurationMinutes { get; set; }
        public double Price { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        public Companion Companion { get; set; }
    }
}
