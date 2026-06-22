using Application.Common.Dto.Result;
using Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Dto
{
    public class CompanionAssistanceUserSearchDto : BaseSearchDto<CompanionAssistanceUser, CompanionAssistanceUserVDto>, ICompanionAssistanceUserSearchFields
    {
        public CompanionAssistanceUserSearchDto(CompanionAssistanceUserInputDto dto, IQueryable<CompanionAssistanceUser> list, IMapper mapper) : base(dto, list, mapper)
        {

            this.CompanionAssistanceId = dto.CompanionAssistanceId;
            this.UserId = dto.UserId;
            this.CompanionId = dto.CompanionId;
        }

        public long? CompanionAssistanceId { get; set; }
        public long? UserId { get; set; }
        public long? CompanionId { get; set; }
    }
}
