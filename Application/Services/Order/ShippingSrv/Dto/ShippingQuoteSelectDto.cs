using System;

namespace Application.Services.Order.ShippingSrv.Dto
{
    public class ShippingQuoteSelectDto
    {
        public Guid QuoteToken { get; set; }

        /// <summary>فقط برای ارسال با میاره الزامی است: بازه‌ی تحویل (از GET ShippingSlot).</summary>
        public long? SlotId { get; set; }

        /// <summary>تاریخ روز تحویل (فقط روز) که همراه SlotId از GET ShippingSlot گرفته شده.</summary>
        public DateTime? SlotDate { get; set; }
    }
}
