using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.PastilMatchField
{
    public class PastilMatchReport : Id_Field
    {
        public long ReporterUserId { get; set; }
        public long ReportedUserId { get; set; }
        public long? ReportedProfileId { get; set; }
        public long? PastilMatchId { get; set; }
        public long? PastilMatchMessageId { get; set; }

        public long PastilMatchReportReasonId { get; set; }
        public string Description { get; set; }
        public string AdminDescription { get; set; }

        public DateTime? ReviewDate { get; set; }
        public DateTime CreateDate { get; set; }

        public User ReporterUser { get; set; }
        public User ReportedUser { get; set; }

        public PastilMatchProfile ReportedProfile { get; set; }
        public PastilMatch PastilMatch { get; set; }
        public PastilMatchMessage PastilMatchMessage { get; set; }

        public PastilMatchReportReason PastilMatchReportReason { get; set; }
    }
}
