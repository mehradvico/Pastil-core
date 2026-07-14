using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageAttachmentSrv.Iface
{
    public interface IPastilMatchMessageAttachmentSearchFields
    {
        public long? PastilMatchMessageId { get; set; }
        public string ContentType { get; set; }
    }
}
