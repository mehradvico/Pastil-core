using Application.Common.Dto.Input;
using Application.Services.PastilMatchSrvs.PastilMatchMessageAttachmentSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageAttachmentSrv.Dto
{
    public class PastilMatchMessageAttachmentInputDto : BaseInputDto, IPastilMatchMessageAttachmentSearchFields
    {
        public long? PastilMatchMessageId { get; set; }
        public string ContentType { get; set; }
    }
}
