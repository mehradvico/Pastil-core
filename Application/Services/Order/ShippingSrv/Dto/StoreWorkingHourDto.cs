using System;

namespace Application.Services.Order.ShippingSrv.Dto
{
    // ساعت کاری یک روز فروشگاه؛ ساعت‌ها "HH:mm" به وقت تهران. روزی که در لیست نیست = تعطیل
    public class StoreWorkingHourDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
    }
}
