using Application.Common.Dto.Field;

namespace Application.Services.CompanionSrv.CompanionTimeSrv.Dto
{
    public class CompanionTimeDto : Id_FieldDto
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Capacity { get; set; }
        public bool Active { get; set; }
        public long WeekDayId { get; set; }
        public long CompanionId { get; set; }
    }
}
