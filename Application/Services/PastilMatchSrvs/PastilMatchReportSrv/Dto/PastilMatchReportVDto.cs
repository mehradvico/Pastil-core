using Application.Common.Dto.Field;
using Application.Services.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Dto
{
    public class PastilMatchReportVDto : Id_FieldDto
    {
        public long ReporterUserId { get; set; }
        public long ReportedUserId { get; set; }
        public long? ReportedProfileId { get; set; }
        public long? PastilMatchId { get; set; }
        public long? PastilMatchMessageId { get; set; }
        public long PastilMatchReportReasonId { get; set; }

        public string Description { get; set; }
        public string AdminDescription { get; set; }
        public string PastilMatchReportReasonTitle { get; set; }
        public string PastilMatchReportReasonDescription { get; set; }

        public DateTime? ReviewDate { get; set; }
        public DateTime CreateDate { get; set; }

        public UserVDto ReporterUser { get; set; }
        public UserVDto ReportedUser { get; set; }
        public PastilMatchProfileVDto ReportedProfile { get; set; }
        public PastilMatchDto PastilMatch { get; set; }
    }
}
