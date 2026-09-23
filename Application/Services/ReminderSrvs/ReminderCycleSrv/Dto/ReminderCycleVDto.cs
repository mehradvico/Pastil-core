using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ReminderSrvs.ReminderCycleSrv.Dto
{
    public class ReminderCycleVDto : Name_FieldDto
    {
        public int Cycle { get; set; }
        public int UnitId { get; set; }
    }
}
