using Application.Common.Dto.Field;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.ReminderSrvs.ReminderCycleSrv.Dto
{
    public class ReminderCycleDto : Name_FieldDto
    {
        [Range(1, int.MaxValue)]
        public int Cycle { get; set; }

        /// <summary>Application.Common.Enumerable.ReminderCycleUnitEnum: 1=روز، 2=هفته، 3=ماه.</summary>
        [Range(1, 3)]
        public int UnitId { get; set; } = (int)Application.Common.Enumerable.ReminderCycleUnitEnum.Month;
    }
}
