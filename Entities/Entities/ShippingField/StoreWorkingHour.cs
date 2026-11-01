using Entities.Entities.CommonField;
using System;

namespace Entities.Entities.ShippingField
{
    /// <summary>
    /// ساعت کاری هفتگی فروشگاه (یک بازه برای هر روز، به وقت تهران).
    /// فروشگاهی که هیچ ردیفی ندارد محدودیت روزانه ندارد؛ اگر ردیف داشت، اکسپرس فقط در همان روزها و ساعت‌ها ارائه می‌شود.
    /// </summary>
    public class StoreWorkingHour : Id_Field
    {
        public long StoreId { get; set; }

        /// <summary>روز هفته (System.DayOfWeek: یکشنبه = 0 ... شنبه = 6).</summary>
        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }

        public Store Store { get; set; }
    }
}
