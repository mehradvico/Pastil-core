using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchSrv.Iface
{
    public interface IPastilMatchSearchFields
    {
        public long? PastilMatchRequestId { get; set; }
        public long? PastilMatchProfileId { get; set; }
        public long? PastilMatchGoalId { get; set; }
        public long? StatusId { get; set; }
    }
}
