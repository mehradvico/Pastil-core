using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Dto
{
    public class PastilMatchMessageReactionItemVDto : Id_FieldDto
    {
        public long ReactorProfileId { get; set; }
        public string Reaction { get; set; }
    }
}
