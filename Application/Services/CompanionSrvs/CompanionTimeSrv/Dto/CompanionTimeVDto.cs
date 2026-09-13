using Application.Common.Dto.Field;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.WeekDaySrv.WeekDaySrv.Dto;

namespace Application.Services.CompanionSrv.CompanionTimeSrv.Dto
{
    public class CompanionTimeVDto : Id_FieldDto
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Capacity { get; set; }
        public long WeekDayId { get; set; }
        public long CompanionId { get; set; }
        public bool Active { get; set; }
        public WeekDayVDto WeekDay { get; set; }
        public CompanionVDto Companion { get; set; }

        // فقط برای نمایش مشتری‌محور (Search سفارشی) پر می‌شود؛ نشان می‌دهد این بازه‌ی
        // زمانی برای نزدیک‌ترین وقوع آینده‌اش (مثلاً «شنبه بعدی») چند ظرفیت خالی دارد.
        public int? RemainingCapacity { get; set; }
        public bool IsFull { get; set; }
    }
}
