using Application.Common.Dto.Field;
using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ReminderSrvs.ReminderCycleSrv.Dto
{
    public class ReminderCycleDto : Name_FieldDto
    {
        [Range(1, int.MaxValue)]
        public int Cycle { get; set; }
    }
}
