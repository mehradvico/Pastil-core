using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchSrv.Dto
{
    public class PastilMatchSearchDto : BaseSearchDto<PastilMatch, PastilMatchVDto>, IPastilMatchSearchFields
    {
        public PastilMatchSearchDto(PastilMatchInputDto dto, IQueryable<PastilMatch> list, IMapper mapper) : base(dto, list, mapper)
        {
            PastilMatchRequestId = dto.PastilMatchRequestId;
            PastilMatchProfileId = dto.PastilMatchProfileId;
            PastilMatchGoalId = dto.PastilMatchGoalId;
            StatusId = dto.StatusId;
        }

        public long? PastilMatchRequestId { get; set; }
        public long? PastilMatchProfileId { get; set; }
        public long? PastilMatchGoalId { get; set; }
        public long? StatusId { get; set; }
    }
}
