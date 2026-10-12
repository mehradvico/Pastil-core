using Entities.Entities.ShippingField;
using System;

namespace Application.Services.Order.ShippingSrv.Dto
{
    public class ShippingQuoteVDto
    {
        public Guid QuoteToken { get; set; }
        public long DeliveryId { get; set; }
        public string DeliveryName { get; set; }
        public ShippingProviderEnum Provider { get; set; }
        public ShippingPaymentModeEnum PaymentMode { get; set; }
        public double QuotedPrice { get; set; }
        public double PayableDeliveryPrice { get; set; }
        public bool PayAtDestination { get; set; }
        public string Currency { get; set; }
        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>true یعنی کاربر باید روز و بازه‌ی تحویل را هم انتخاب کند (میاره)؛ false (مثلاً پست) بدون زمان‌بندی است.</summary>
        public bool RequiresSlot { get; set; }

        /// <summary>گزینه‌ی «ارسال اکسپرس»: بدون روز/بازه، پیک همان لحظه؛ QuotedPrice شامل اضافه‌ی اکسپرس است.</summary>
        public bool IsExpress { get; set; }
        /// <summary>اضافه‌ی اکسپرس داخل QuotedPrice (تومان)</summary>
        public double ExpressSurcharge { get; set; }
        /// <summary>متن آماده‌ی نمایش، مثلاً «۲ تا ۳ ساعت کاری»</summary>
        public string ExpressText { get; set; }
    }
}
