using Application.Common.Dto.Input;
using Application.Services.CompanionSrv.CompanionTimeSrv.Iface;

namespace Application.Services.CompanionSrv.CompanionTimeSrv.Dto
{
    public class CompanionTimeInputDto : BaseInputDto, ICompanionTimeSearchFields
    {
        public long? WeekDayId { get; set; }
        public long? CompanionId { get; set; }
        public bool? Active { get; set; }
    }
}
