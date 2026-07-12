using Application.Common.Dto.Input;
using Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv.Dto
{
    public class PastilMatchProfileGoalInputDto : BaseInputDto, IPastilMatchProfileGoalSearchFields
    {
        public long? PastilMatchProfileId { get; set; }
        public long? PastilMatchGoalId { get; set; }
    }
}
