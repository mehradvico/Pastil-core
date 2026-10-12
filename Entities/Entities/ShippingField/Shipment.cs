using Entities.Entities.CommonField;
using System;

namespace Entities.Entities.ShippingField
{
    public class Shipment : Id_Field
    {
        public long ProductOrderStoreId { get; set; }
        public long? ShippingQuoteId { get; set; }
        public ShippingProviderEnum Provider { get; set; }
        public ShippingPaymentModeEnum PaymentMode { get; set; }
        public ShipmentStatusEnum Status { get; set; }
        public double QuotedPrice { get; set; }
        public double ChargedPrice { get; set; }
        public double? ProviderCost { get; set; }
        public string ExternalShipmentId { get; set; }
        public string TrackingCode { get; set; }
        public string FailureReason { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? RequestedAtUtc { get; set; }
        public DateTime? DeliveredAtUtc { get; set; }

        // بازه‌ی تحویل انتخاب‌شده‌ی مشتری (snapshot به UTC؛ ویرایش بعدی بازه‌ها روی مرسوله‌ی ثبت‌شده اثر ندارد)
        public DateTime? SlotStartUtc { get; set; }
        public DateTime? SlotEndUtc { get; set; }
        /// <summary>مهلت تأیید فروشنده؛ بعد از آن سفارش خودکار لغو می‌شود.</summary>
        public DateTime? SellerConfirmDeadlineUtc { get; set; }
        public DateTime? SellerConfirmedAtUtc { get; set; }
        /// <summary>ساعتی که فروشنده گفته کالا آماده‌ی تحویل به پیک است (= pickup.deadline میاره).</summary>
        public DateTime? PickupDeadlineUtc { get; set; }
        public DateTime? SellerReminderSentAtUtc { get; set; }
        /// <summary>کد ۵ رقمی تحویل (PoD)؛ مشتری در اپ می‌بیند و به پیک می‌گوید.</summary>
        public string DeliveryCode { get; set; }

        /// <summary>لحظه‌ی «تحویل پیک شد»؛ پیامک/پوش ارسال فقط یک‌بار (وب‌هوک میاره تکرار می‌شود).</summary>
        public DateTime? ShippedNotifiedAtUtc { get; set; }
        /// <summary>چند بار بعد از لغو سفر (تأخیر) دوباره از فروشنده تأیید گرفته شده؛ حداکثر ۱ بار.</summary>
        public int CourierRetryCount { get; set; }
        /// <summary>هشدار تأخیر نسبت به بازه‌ی تحویل فقط یک‌بار.</summary>
        public DateTime? LateNotifiedAtUtc { get; set; }
        /// <summary>پوش «فروشنده هنوز سفارش را آماده نکرده» به ادمین‌ها (۳۰ دقیقه بعد از پرداخت) فقط یک‌بار.</summary>
        public DateTime? SellerNotReadyAdminNotifiedAtUtc { get; set; }
        /// <summary>مشتری بعد از تحویل میاره «تحویل نگرفتم» زده و به ادمین اطلاع داده شده.</summary>
        public DateTime? DisputeReportedAtUtc { get; set; }

        /// <summary>آخرین ساعتی که فروشنده باید «آماده تحویل به پیک» بزند تا هنوز وقت رساندن داخل بازه باشد.</summary>
        public DateTime? ReadyDeadlineUtc { get; set; }
        /// <summary>لحظه‌ای که فروشنده «آماده تحویل به پیک» زد (همان لحظه سفر میاره ساخته می‌شود).</summary>
        public DateTime? ReadyAtUtc { get; set; }
        public DateTime? ReadyReminderSentAtUtc { get; set; }

        /// <summary>ارسال اکسپرس: فروشنده فقط یک مرحله دارد («درخواست پیک») و سفر با Boost ساخته می‌شود.</summary>
        public bool IsExpress { get; set; }
        /// <summary>اضافه‌ی اکسپرس که از مشتری گرفته شد (مبلغ boost یا مبلغ ثابت پاستیل)</summary>
        public double ExpressSurcharge { get; set; }
        /// <summary>سطح و مبلغ boost که واقعاً روی سفر میاره اعمال شد؛ null = اعمال نشد (غیرفعال بودن Boost برای حساب و ...)</summary>
        public int? BoostLevel { get; set; }
        public double? BoostAmount { get; set; }

        public ProductOrderStore ProductOrderStore { get; set; }
        public ShippingQuote ShippingQuote { get; set; }
    }
}
