using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchBlockSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchBlockSrv.Dto
{
    public class PastilMatchBlockSearchDto : BaseSearchDto<PastilMatchBlock, PastilMatchBlockVDto>, IPastilMatchBlockSearchFields
    {
        public PastilMatchBlockSearchDto(PastilMatchBlockInputDto dto, IQueryable<PastilMatchBlock> list, IMapper mapper) : base(dto, list, mapper)
        {
            BlockerUserId = dto.BlockerUserId;
            BlockedUserId = dto.BlockedUserId;
            PastilMatchId = dto.PastilMatchId;
        }

        public long? BlockerUserId { get; set; }
        public long? BlockedUserId { get; set; }
        public long? PastilMatchId { get; set; }
    }
}
