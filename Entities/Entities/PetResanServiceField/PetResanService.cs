using Entities.Entities.CommonField;
using Entities.Entities.Security;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace Entities.Entities.PetResanServiceField
{
    /// <summary>
    /// سرویس پت‌رسان تکرارشونده‌ی هفتگی — کاربر یک‌بار تعریف می‌کند (مبدا/مقصد/پت/روزها)،
    /// و از آن به بعد به‌صورت خودکار هر هفته یک سفر رفت‌وبرگشت واقعی (Trip) ساخته می‌شود.
    /// برخلاف بقیه‌ی حالت‌های پت‌رسان، راننده Broadcast نمی‌شود — پاستیل (ادمین) تخصیص می‌دهد.
    /// </summary>
    public class PetResanService : Id_Field
    {
        public long UserId { get; set; }
        public long UserPetId { get; set; }

        public Point Origin { get; set; }
        public Point Destination { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }

        public DateTime StartDate { get; set; }

        // null یعنی بی‌نهایت/باز (تا خودِ کاربر لغو کند)
        public int? TotalWeeks { get; set; }
        public DateTime? EndDate { get; set; }

        public bool Active { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? CancelDate { get; set; }

        // فقط برای پیش‌نمایش قیمت به کاربر؛ قیمت واقعیِ هر occurrence دوباره در لحظه‌ی
        // ساخت Trip با نرخ همان ساعت محاسبه می‌شود (مثل بقیه‌ی سفرهای پت‌رسان).
        public double PricePerOccurrence { get; set; }

        public User User { get; set; }
        public UserPet UserPet { get; set; }
        public ICollection<PetResanServiceSchedule> Schedules { get; set; }
    }
}
