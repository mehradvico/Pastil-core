using Entities.Entities.CommonField;
using System;

namespace Entities.Entities.ShippingField
{
    // بازه‌ی زمانی تحویل (مثلاً ۹ تا ۱۳) برای یک روز هفته؛ سراسری و از پنل ادمین تنظیم می‌شود.
    // طراحی: backend/Docs/MIARE_SHIPPING_SLOTS_FA.md
    public class ShippingSlot : Id_Field
    {
        /// <summary>روز هفته (System.DayOfWeek: یکشنبه = 0 ... شنبه = 6).</summary>
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>شروع بازه به وقت تهران.</summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>پایان بازه به وقت تهران.</summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>حداکثر تعداد مرسوله‌ی پرداخت‌شده برای این بازه در هر تاریخ.</summary>
        public int Capacity { get; set; }

        public bool Active { get; set; } = true;
        public bool Deleted { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
