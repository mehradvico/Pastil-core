using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchMessageReactionSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageReactionSrv.Dto
{
    public class PastilMatchMessageReactionSearchDto : BaseSearchDto<PastilMatchMessageReaction, PastilMatchMessageReactionVDto>, IPastilMatchMessageReactionSearchFields
    {
        public PastilMatchMessageReactionSearchDto(PastilMatchMessageReactionInputDto dto, IQueryable<PastilMatchMessageReaction> list, IMapper mapper) : base(dto, list, mapper)
        {
            PastilMatchMessageId = dto.PastilMatchMessageId;
            ReactorProfileId = dto.ReactorProfileId;
            Reaction = dto.Reaction;
        }

        public long? PastilMatchMessageId { get; set; }
        public long? ReactorProfileId { get; set; }
        public string Reaction { get; set; }
    }
}
