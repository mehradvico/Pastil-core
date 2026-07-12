using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv.Dto
{
    public class PastilMatchProfileGoalDto : Id_FieldDto
    {
        public long PastilMatchProfileId { get; set; }
        public long PastilMatchGoalId { get; set; }
    }
}
