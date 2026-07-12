using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv.Dto
{
    public class PastilMatchProfileGoalSearchDto : BaseSearchDto<PastilMatchProfileGoal, PastilMatchProfileGoalVDto>, IPastilMatchProfileGoalSearchFields
    {
        public PastilMatchProfileGoalSearchDto(PastilMatchProfileGoalInputDto dto, IQueryable<PastilMatchProfileGoal> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.PastilMatchProfileId = dto.PastilMatchProfileId;
            this.PastilMatchGoalId = dto.PastilMatchGoalId;
        }

        public long? PastilMatchProfileId { get; set; }
        public long? PastilMatchGoalId { get; set; }
    }
}
