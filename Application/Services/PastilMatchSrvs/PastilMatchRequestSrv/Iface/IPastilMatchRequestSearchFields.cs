using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchRequestSrv.Iface
{
    public interface IPastilMatchRequestSearchFields
    {
        public long? SenderProfileId { get; set; }
        public long? ReceiverProfileId { get; set; }
        public long? PastilMatchGoalId { get; set; }
        public long? StatusId { get; set; }
    }
}
