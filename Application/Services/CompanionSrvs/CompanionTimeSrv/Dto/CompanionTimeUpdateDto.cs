using Application.Services.WeekDaySrv.WeekDaySrv.Dto;
using System.Collections.Generic;

namespace Application.Services.CompanionSrv.CompanionTimeSrv.Dto
{
    public class CompanionTimeUpdateDto
    {
        public WeekDayDto WeekDay { get; set; }
        public List<CompanionTimeDto> CompanionTimes { get; set; } = new List<CompanionTimeDto>();
    }
}
