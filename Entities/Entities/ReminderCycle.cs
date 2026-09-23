using Entities.Entities.CommonField;

namespace Entities.Entities
{
    public class ReminderCycle : Name_Field
    {
        /// <summary>تعداد واحد (مثلاً 1، 3، 7)؛ معنای واقعی‌اش با <see cref="UnitId"/> مشخص می‌شود.</summary>
        public int Cycle { get; set; }
        /// <summary>مقدار Application.Common.Enumerable.ReminderCycleUnitEnum (Day=1 / Week=2 / Month=3). چرخه‌های قدیمی همیشه Month بودند.</summary>
        public int UnitId { get; set; }
        public bool Deleted { get; set; }
    }
}
