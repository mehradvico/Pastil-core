using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionTimeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CompanionSrv.CompanionTimeSrv.Dto
{
    public class CompanionTimeSearchDto : BaseSearchDto<CompanionTime, CompanionTimeVDto>, ICompanionTimeSearchFields
    {
        public CompanionTimeSearchDto(CompanionTimeInputDto dto, IQueryable<CompanionTime> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.WeekDayId = dto.WeekDayId;
            this.CompanionId = dto.CompanionId;
            this.Active = dto.Active;
        }

        public long? WeekDayId { get; set; }
        public long? CompanionId { get; set; }
        public bool? Active { get; set; }
    }
}
