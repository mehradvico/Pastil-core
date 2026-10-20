using System;
using System.Collections.Generic;

namespace Application.Services.TripSrv.TripSrv.Dto
{
    /// <summary>
    /// خلاصه‌ی «سرویس هفتگی»ِ یک نوبت برای راننده (فقط برنامه؛ بدون قیمت و اطلاعات شخصی کاربر).
    /// فقط وقتی Trip.PetResanServiceScheduleId پر باشد پر می‌شود.
    /// </summary>
    public class TripServiceInfoVDto
    {
        public long ServiceId { get; set; }
        /// <summary>ردیف برنامه‌ای که همین نوبت از آن ساخته شده</summary>
        public long ScheduleId { get; set; }
        public bool Active { get; set; }
        public DateTime StartDate { get; set; }
        public int? TotalWeeks { get; set; }
        public DateTime? EndDate { get; set; }
        public List<TripServiceScheduleVDto> Schedules { get; set; } = new List<TripServiceScheduleVDto>();
    }

    public class TripServiceScheduleVDto
    {
        public long WeekDayId { get; set; }
        public string WeekDayName { get; set; }
        public int WeekDayNumber { get; set; }
        /// <summary>ساعت رفت HH:mm</summary>
        public string Time { get; set; }
        /// <summary>ساعت برگشت HH:mm یا null</summary>
        public string ReturnTime { get; set; }
    }
}
