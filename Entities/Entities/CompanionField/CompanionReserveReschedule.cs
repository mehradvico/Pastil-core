using Entities.Entities.CommonField;
using System;

namespace Entities.Entities
{
    /// <summary>
    /// تاریخچه‌ی تغییر زمان یک رزرو (کلینیک / مربی / آرایشگاه) توسط نماینده یا ادمین.
    /// هر تغییر یک ردیف دارد: زمان قبلی، زمان جدید، دلیل و اینکه چه کسی تغییر داده.
    /// </summary>
    public class CompanionReserveReschedule : Id_Field
    {
        public long CompanionReserveId { get; set; }

        // روز رزرو (بدون ساعت) و بازه‌ی ساعتی (CompanionTime) قبل و بعد از تغییر
        public DateTime OldDoDate { get; set; }
        public long? OldCompanionTimeId { get; set; }
        public DateTime NewDoDate { get; set; }
        public long? NewCompanionTimeId { get; set; }

        // زمان کامل شروع (روز + ساعت شروع بازه)؛ برای نمایش و گزارش بدون نیاز به join
        public DateTime? OldStartAt { get; set; }
        public DateTime NewStartAt { get; set; }

        public string Reason { get; set; }

        // 1 = نماینده، 2 = ادمین
        public int ActorKind { get; set; }
        public long ActorUserId { get; set; }

        // سفر پت‌رسانِ متصل که همراه این تغییر جابه‌جا شد (اگر بود) و اینکه راننده‌اش پیامک گرفت یا نه
        public long? LinkedTripId { get; set; }
        public bool UserNotified { get; set; }
        public bool DriverNotified { get; set; }

        public DateTime CreateDate { get; set; }

        public CompanionReserve CompanionReserve { get; set; }
    }
}
