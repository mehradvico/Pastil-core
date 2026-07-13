using Application.Common.Dto.Input;
using Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Dto
{
    public class PastilMatchRequestInputDto : BaseInputDto, IPastilMatchRequestSearchFields
    {
        public long? SenderProfileId { get; set; }
        public long? ReceiverProfileId { get; set; }
        public long? PastilMatchGoalId { get; set; }
        public long? StatusId { get; set; }
    }
}
