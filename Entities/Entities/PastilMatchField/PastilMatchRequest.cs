using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.PastilMatchField
{
    public class PastilMatchRequest : Id_Field
    {
        public long SenderProfileId { get; set; }
        public long ReceiverProfileId { get; set; }
        public long PastilMatchGoalId { get; set; }
        public long StatusId { get; set; }

        public string Description { get; set; }
        public int CompatibilityPercent { get; set; }

        public DateTime? ResponseDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? CancelDate { get; set; }

        public PastilMatchProfile SenderProfile { get; set; }
        public PastilMatchProfile ReceiverProfile { get; set; }
        public Code PastilMatchGoal { get; set; }
        public Code Status { get; set; }
    }
}
