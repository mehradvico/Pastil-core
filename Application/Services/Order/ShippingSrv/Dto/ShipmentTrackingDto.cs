using System;
using System.Collections.Generic;

namespace Application.Services.Order.ShippingSrv.Dto
{
    // ردیابی مرسوله‌ی پیک (میاره) برای مشتری و فروشنده. زمان‌ها UTC با Kind=Utc (JSON با Z).
    // قرارداد کامل برای اپ/وب‌اپ: backend/Docs/MIARE_COURIER_TRACKING_FA.md
    public class ShipmentTrackingVDto
    {
        public long ShipmentId { get; set; }
        public long ProductOrderStoreId { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string Provider { get; set; }
        /// <summary>ShipmentStatusEnum: 8/9 آماده‌سازی، 2 پیک درخواست شد، 3 پیک تعیین شد، 4 در راه، 5 تحویل شد، 6 لغو، 7 ناموفق.</summary>
        public int Status { get; set; }
        public string StatusKey { get; set; }
        public bool IsExpress { get; set; }
        /// <summary>true = دیگر تغییر نمی‌کند (تحویل/لغو/ناموفق)؛ polling را متوقف کنید.</summary>
        public bool IsFinal { get; set; }
        /// <summary>true = پیک در راه است و موقعیت زنده معنی دارد (وضعیت ۳ یا ۴).</summary>
        public bool IsLive { get; set; }
        public string CurrentStepKey { get; set; }
        /// <summary>متن فارسی مرحله‌ی جاری برای نمایش در بالای صفحه.</summary>
        public string Headline { get; set; }
        public List<ShipmentTrackStepVDto> Steps { get; set; } = new();
        /// <summary>null = پیک هنوز تعیین نشده یا اطلاعاتش برای این مخاطب/وضعیت نمایش داده نمی‌شود.</summary>
        public ShipmentCourierVDto Courier { get; set; }
        /// <summary>نقطه‌ی فروشگاه (مبدأ) روی نقشه؛ ممکن است null باشد.</summary>
        public ShipmentPointVDto Store { get; set; }
        /// <summary>آدرس تحویل؛ فقط در پاسخ مشتری (فروشنده هرگز مختصات مشتری را نمی‌گیرد).</summary>
        public ShipmentPointVDto Destination { get; set; }
        public DateTime? PlannedPickupUtc { get; set; }
        public DateTime? DeliveredAtUtc { get; set; }
        public DateTime? LastUpdateUtc { get; set; }
        public int PollSeconds { get; set; }
    }

    public class ShipmentTrackStepVDto
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public bool Done { get; set; }
        public bool Current { get; set; }
        public DateTime? AtUtc { get; set; }
    }

    public class ShipmentCourierVDto
    {
        /// <summary>مشتری فقط نام کوچک می‌بیند؛ فروشنده نام کامل.</summary>
        public string Name { get; set; }
        public string Phone { get; set; }
        public bool CanCall { get; set; }
        public bool HasLocation { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime? LocationUpdatedAtUtc { get; set; }
        public int? LocationAgeSeconds { get; set; }
        /// <summary>موقعیت قدیمی‌تر از ۱۰ دقیقه؛ روی نقشه «آخرین موقعیت معلوم» نشان دهید.</summary>
        public bool LocationStale { get; set; }
        /// <summary>فاصله‌ی هوایی (نه مسیر) پیک تا هدف، به کیلومتر.</summary>
        public double? DistanceKm { get; set; }
        /// <summary>هدفِ DistanceKm: "store" (در مسیر فروشگاه) یا "destination" (در مسیر مشتری).</summary>
        public string DistanceTarget { get; set; }
    }

    public class ShipmentPointVDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
