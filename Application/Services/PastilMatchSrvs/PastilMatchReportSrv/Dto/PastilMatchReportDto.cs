using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Dto
{
    public class PastilMatchReportDto : Id_FieldDto
    {
        public long ReportedUserId { get; set; }
        public long? ReportedProfileId { get; set; }
        public long? PastilMatchId { get; set; }
        public long? PastilMatchMessageId { get; set; }
        public long PastilMatchReportReasonId { get; set; }
        public string Description { get; set; }
    }
}
