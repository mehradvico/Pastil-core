using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Iface
{
    public interface IPastilMatchMessageSearchFields
    {
        public long? PastilMatchId { get; set; }
        public long? SenderProfileId { get; set; }
        public long? PastilMatchMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public long? ParkId { get; set; }
        public bool? IsPinned { get; set; }
        public bool? IsRead { get; set; }
        public long? BeforeMessageId { get; set; }
        public long? AfterMessageId { get; set; }
    }
}
