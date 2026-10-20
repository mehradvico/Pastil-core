using Entities.Entities.CoachField;
using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachReserveMessageReactionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CoachSrvs.CoachReserveMessageReactionSrv.Dto
{
    public class CoachReserveMessageReactionSearchDto : BaseSearchDto<CoachReserveMessageReaction, CoachReserveMessageReactionVDto>, ICoachReserveMessageReactionSearchFields
    {
        public CoachReserveMessageReactionSearchDto(CoachReserveMessageReactionInputDto dto, IQueryable<CoachReserveMessageReaction> list, IMapper mapper) : base(dto, list, mapper)
        {
            CoachReserveMessageId = dto.CoachReserveMessageId;
            ReactorUserId = dto.ReactorUserId;
            Reaction = dto.Reaction;
        }

        public long? CoachReserveMessageId { get; set; }
        public long? ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
