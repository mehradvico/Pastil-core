using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.CompanionSrv.CompanionReserveSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CompanionSrv.CompanionReserveSrv.Dto
{
    public class CompanionReserveSearchDto : BaseSearchDto<CompanionReserve, CompanionReserveVDto>, ICompanionReserveSearchFields
    {
        public CompanionReserveSearchDto(CompanionReserveInputDto dto, IQueryable<CompanionReserve> list, IMapper mapper) : base(dto, list, mapper)
        {

            this.BookerId = dto.BookerId;
            this.UserPetId = dto.UserPetId;
            this.CompanionAssistanceId = dto.CompanionAssistanceId;
            this.CompanionAssistanceTimeId = dto.CompanionAssistanceTimeId;
            this.CompanionTimeId = dto.CompanionTimeId;
            this.CompanionAssistanceUserId = dto.CompanionAssistanceUserId;
            this.IsFemale = dto.IsFemale;
            this.CompanionId = dto.CompanionId;
            this.ReserveState = dto.ReserveState;
        }

        public long? BookerId { get; set; }
        public long? UserPetId { get; set; }
        public long? CompanionAssistanceId { get; set; }
        public long? CompanionAssistanceTimeId { get; set; }
        public long? CompanionTimeId { get; set; }
        public long? CompanionAssistanceUserId { get; set; }
        public bool? IsFemale { get; set; }
        public long? CompanionId { get; set; }
        public ReserveStateEnum? ReserveState { get; set; }

    }
}
