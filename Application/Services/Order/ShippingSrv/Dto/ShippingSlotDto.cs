using System;
using System.Collections.Generic;

namespace Application.Services.Order.ShippingSrv.Dto
{
    // ورودی/خروجی مدیریت بازه‌ها در پنل ادمین. ساعت‌ها به‌صورت "HH:mm" (وقت تهران)
    public class ShippingSlotDto
    {
        public long Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Capacity { get; set; }
        public bool Active { get; set; } = true;
    }

    // برای مشتری: روزهای قابل انتخاب و بازه‌های هر روز با ظرفیت باقی‌مانده
    public class ShippingSlotDayVDto
    {
        /// <summary>تاریخ میلادی (فقط روز) که باید همراه SlotId برای انتخاب ارسال شود.</summary>
        public DateTime Date { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public List<ShippingSlotOptionVDto> Slots { get; set; } = new();
    }

    public class ShippingSlotOptionVDto
    {
        public long SlotId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Remaining { get; set; }
        public bool Available { get; set; }
    }
}
