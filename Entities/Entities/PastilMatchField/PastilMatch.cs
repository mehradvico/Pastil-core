using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.PastilMatchField
{
    public class PastilMatch : Id_Field
    {
        public long PastilMatchRequestId { get; set; }
        public long FirstProfileId { get; set; }
        public long SecondProfileId { get; set; }
        public long PastilMatchGoalId { get; set; }
        public long StatusId { get; set; }

        public int CompatibilityPercent { get; set; }

        public DateTime? CloseDate { get; set; }
        public DateTime CreateDate { get; set; }

        public PastilMatchRequest PastilMatchRequest { get; set; }
        public PastilMatchProfile FirstProfile { get; set; }
        public PastilMatchProfile SecondProfile { get; set; }
        public Code PastilMatchGoal { get; set; }
        public Code Status { get; set; }
    }
}
