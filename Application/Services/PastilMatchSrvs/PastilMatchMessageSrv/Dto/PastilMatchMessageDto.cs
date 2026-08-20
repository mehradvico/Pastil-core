using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Dto
{
    public class PastilMatchMessageDto : Id_FieldDto
    {
        public long PastilMatchId { get; set; }
        public long? SenderProfileId { get; set; }
        public long PastilMatchMessageTypeId { get; set; }
        public long? ReplyToMessageId { get; set; }
        public long? ParkId { get; set; }
        public string Content { get; set; }
    }
}
