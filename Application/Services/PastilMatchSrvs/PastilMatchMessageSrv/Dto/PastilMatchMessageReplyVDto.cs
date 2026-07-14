using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Dto
{
    public class PastilMatchMessageReplyVDto : Id_FieldDto
    {
        public long? SenderProfileId { get; set; }
        public long PastilMatchMessageTypeId { get; set; }
        public string Content { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
