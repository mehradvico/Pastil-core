using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchSrv.Dto
{
    public class PastilMatchDto : Id_FieldDto
    {
        public long PastilMatchRequestId { get; set; }
        public long FirstProfileId { get; set; }
        public long SecondProfileId { get; set; }
        public long PastilMatchGoalId { get; set; }
        public long StatusId { get; set; }

        public int CompatibilityPercent { get; set; }

        public DateTime? CloseDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
