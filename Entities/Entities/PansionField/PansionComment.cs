using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.PansionField
{
    public class PansionComment : Comment
    {
        public long PansionId { get; set; }
        public long? PansionReserveId { get; set; }
        public bool IsReserved { get; set; }
        public Pansion Pansion { get; set; }
        public PansionReserve PansionReserve { get; set; }
    }
}
