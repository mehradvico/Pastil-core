using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv.Iface
{
    public interface IPastilMatchProfileGoalSearchFields
    {
        public long? PastilMatchProfileId { get; set; }
        public long? PastilMatchGoalId { get; set; }
    }
}
