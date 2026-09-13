using Entities.Entities.CommonField;

namespace Entities.Entities
{
    public class CompanionTime : Id_Field
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Capacity { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public long WeekDayId { get; set; }
        public long CompanionId { get; set; }
        public WeekDay WeekDay { get; set; }
        public Companion Companion { get; set; }
    }
}
