using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageReactionSrv.Dto
{
    public class PastilMatchMessageReactionDto : Id_FieldDto
    {
        public long PastilMatchMessageId { get; set; }
        public string Reaction { get; set; }
    }
}
