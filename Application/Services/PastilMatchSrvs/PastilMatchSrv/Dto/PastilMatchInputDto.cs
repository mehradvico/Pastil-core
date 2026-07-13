using Application.Common.Dto.Input;
using Application.Services.PastilMatchSrvs.PastilMatchSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchSrv.Dto
{
    public class PastilMatchInputDto : BaseInputDto, IPastilMatchSearchFields
    {
        public long? PastilMatchRequestId { get; set; }
        public long? PastilMatchProfileId { get; set; }
        public long? PastilMatchGoalId { get; set; }
        public long? StatusId { get; set; }
    }
}
