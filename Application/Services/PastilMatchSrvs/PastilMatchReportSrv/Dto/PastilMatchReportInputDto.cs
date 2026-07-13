using Application.Common.Dto.Input;
using Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchReportSrv.Dto
{
    public class PastilMatchReportInputDto : BaseInputDto, IPastilMatchReportSearchFields
    {
        public long? ReporterUserId { get; set; }
        public long? ReportedUserId { get; set; }
        public long? ReportedProfileId { get; set; }
        public long? PastilMatchId { get; set; }
        public long? PastilMatchMessageId { get; set; }
        public long? PastilMatchReportReasonId { get; set; }
        public bool? IsReviewed { get; set; }
    }
}
