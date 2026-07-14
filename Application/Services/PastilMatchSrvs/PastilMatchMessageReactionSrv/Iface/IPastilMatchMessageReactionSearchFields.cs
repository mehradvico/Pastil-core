using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageReactionSrv.Iface
{
    public interface IPastilMatchMessageReactionSearchFields
    {
        public long? PastilMatchMessageId { get; set; }
        public long? ReactorProfileId { get; set; }
        public string Reaction { get; set; }
    }
}
