using System.Collections.Generic;

namespace Application.Services.CompanionSrv.CompanionTimeSrv.Dto
{
    public class CompanionTimeUpdateListDto
    {
        public long CompanionId { get; set; }
        public List<CompanionTimeUpdateDto> CompanionTimeUpdateList { get; set; }
    }
}
