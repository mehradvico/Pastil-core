using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Dto
{
    public class PastilMatchRequestSearchDto : BaseSearchDto<PastilMatchRequest, PastilMatchRequestVDto>, IPastilMatchRequestSearchFields
    {
        public PastilMatchRequestSearchDto(PastilMatchRequestInputDto dto, IQueryable<PastilMatchRequest> list, IMapper mapper) : base(dto, list, mapper)
        {
            SenderProfileId = dto.SenderProfileId;
            ReceiverProfileId = dto.ReceiverProfileId;
            PastilMatchGoalId = dto.PastilMatchGoalId;
            StatusId = dto.StatusId;
        }

        public long? SenderProfileId { get; set; }
        public long? ReceiverProfileId { get; set; }
        public long? PastilMatchGoalId { get; set; }
        public long? StatusId { get; set; }
    }
}
