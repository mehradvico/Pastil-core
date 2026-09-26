using Entities.Entities.CommonField;
using System;

namespace Entities.Entities
{
    // پکیج «مشاوره آنلاین» نام‌دار یک کلینیک: نام + کانال + مدت (از فهرست ثابت) + قیمت + تصویر. کاملاً جدا از CompanionAssistancePackage است
    // (طراحی: backend/Docs/ONLINE_CONSULTATION_PACKAGES_FA.md). هر کلینیک زیر هر کانال هر تعداد پکیج می‌تواند داشته باشد.
    public class ConsultationPackage : Id_Field
    {
        public long CompanionId { get; set; }
        // مقدار OnlineSessionChannelEnum (۱ چت، ۲ تماس درون‌برنامه، ۳ تماس تصویری، ۴ تماس تلفنی)
        public int ChannelId { get; set; }
        // یکی از مدت‌های ثابت (ConsultationRules.AllowedDurations)
        public int DurationMinutes { get; set; }
        // نام پکیج که کاربر می‌بیند (الزامی)، توضیح کوتاه و تصویر اختیاری، و ترتیب نمایش زیر هر کانال
        public string Name { get; set; }
        public string Description { get; set; }
        public long? PictureId { get; set; }
        public int SortOrder { get; set; }
        public double Price { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        public Companion Companion { get; set; }
        public Picture Picture { get; set; }
    }
}
