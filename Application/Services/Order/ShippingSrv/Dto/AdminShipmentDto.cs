using System;
using System.Collections.Generic;

namespace Application.Services.Order.ShippingSrv.Dto
{
    // مرسوله‌ی یک سفارش برای پنل ادمین (زمان‌ها UTC با Kind=Utc تا JSON با Z برود)
    public class AdminShipmentVDto
    {
        public long Id { get; set; }
        public long ProductOrderStoreId { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string Provider { get; set; }
        public int Status { get; set; }
        public string StatusKey { get; set; }
        public bool IsExpress { get; set; }
        public string FailureReason { get; set; }
        public string TrackingCode { get; set; }
        public string ExternalShipmentId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? SellerConfirmDeadlineUtc { get; set; }
        public DateTime? ReadyDeadlineUtc { get; set; }
        public DateTime? PickupDeadlineUtc { get; set; }
        public DateTime? RequestedAtUtc { get; set; }
        // دکمه‌ی «تمدید مهلت فروشنده» فقط وقتی true است
        public bool CanExtendSellerDeadline { get; set; }

        // آخرین اطلاعات پیک از وب‌هوک میاره (ادمین همه‌چیز را می‌بیند) و تاریخچه‌ی رویدادها
        public string CourierName { get; set; }
        public string CourierPhone { get; set; }
        public double? CourierLatitude { get; set; }
        public double? CourierLongitude { get; set; }
        public DateTime? CourierLocationAtUtc { get; set; }
        public string LastMiareState { get; set; }
        public DateTime? LastMiareEventAtUtc { get; set; }
        // مبدأ (فروشگاه) و مقصد (آدرس مشتری) برای نقشه‌ی پنل؛ اگر ثبت نشده باشد null
        public double? StoreLatitude { get; set; }
        public double? StoreLongitude { get; set; }
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }
        public List<AdminShipmentEventVDto> Events { get; set; } = new();
    }

    public class AdminShipmentEventVDto
    {
        public long Id { get; set; }
        public string State { get; set; }
        public DateTime ReceivedAtUtc { get; set; }
        public string CourierName { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        /// <summary>متن خام وب‌هوک (بدون کد تحویل)؛ برای عیب‌یابی نام فیلدهای میاره.</summary>
        public string PayloadJson { get; set; }
    }

    public class AdminExtendShipmentDeadlineDto
    {
        public long ShipmentId { get; set; }
        public int Minutes { get; set; } = 60;
    }
}
