using Entities.Entities.CommonField;
using System;

namespace Entities.Entities.ShippingField
{
    /// <summary>
    /// یک رویداد وب‌هوک میاره برای مرسوله (تغییر وضعیت، تخصیص پیک، موقعیت). برای نمایش «مراحل» در پنل/اپ و عیب‌یابی
    /// (متن خام وب‌هوک بعد از حذف کد تحویل). رویداد تکراری (retry میاره) و موقعیت‌های پشت‌سرهم ذخیره نمی‌شوند؛ ShipmentCourierRules.
    /// </summary>
    public class ShipmentEvent : Id_Field
    {
        public long ShipmentId { get; set; }
        /// <summary>state میاره همان‌طور که رسید (حروف کوچک)؛ مثل assign_queue / pickup / dropoff / delivered.</summary>
        public string State { get; set; }
        public DateTime ReceivedAtUtc { get; set; }
        public string CourierName { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        /// <summary>SHA-256 متن خام (برای تشخیص retry تکراری میاره).</summary>
        public string PayloadHash { get; set; }
        /// <summary>متن خام وب‌هوک بعد از پاک‌سازی (بدون کد تحویل) و کوتاه‌شده؛ فقط برای ادمین دیده می‌شود.</summary>
        public string PayloadJson { get; set; }

        public Shipment Shipment { get; set; }
    }
}
