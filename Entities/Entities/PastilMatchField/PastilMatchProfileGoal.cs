using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.PastilMatchField
{
    public class PastilMatchProfileGoal : Id_Field
    {
        public long PastilMatchProfileId { get; set; }
        public long PastilMatchGoalId { get; set; }

        public bool Deleted { get; set; }
   
        public PastilMatchProfile PastilMatchProfile { get; set; }
        public Code PastilMatchGoal { get; set; }
    }
}
