using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveMessageReactionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageReactionSrv.Dto
{
    public class CompanionReserveMessageReactionSearchDto : BaseSearchDto<CompanionReserveMessageReaction, CompanionReserveMessageReactionVDto>, ICompanionReserveMessageReactionSearchFields
    {
        public CompanionReserveMessageReactionSearchDto(CompanionReserveMessageReactionInputDto dto, IQueryable<CompanionReserveMessageReaction> list, IMapper mapper) : base(dto, list, mapper)
        {
            CompanionReserveMessageId = dto.CompanionReserveMessageId;
            ReactorUserId = dto.ReactorUserId;
            Reaction = dto.Reaction;
        }

        public long? CompanionReserveMessageId { get; set; }
        public long? ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
