using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchBlockSrv.Iface
{
    public interface IPastilMatchBlockSearchFields
    {
        public long? BlockerUserId { get; set; }
        public long? BlockedUserId { get; set; }
        public long? PastilMatchId { get; set; }
    }
}
