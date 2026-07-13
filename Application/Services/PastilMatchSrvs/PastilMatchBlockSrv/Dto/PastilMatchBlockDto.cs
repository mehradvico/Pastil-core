using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchBlockSrv.Dto
{
    public class PastilMatchBlockDto : Id_FieldDto
    {
        public long BlockedUserId { get; set; }
        public long? PastilMatchId { get; set; }
    }
}
