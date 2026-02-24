using Entities.Entities.CommonField;
using Entities.Entities.PansionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class SettlementCompanion : Id_Field
    {
        public long? CompanionReserveId { get; set; }
        public long? PansionReserveId { get; set; }
        public long SettlementId { get; set; }

        public Settlement Settlement { get; set; }
        public PansionReserve PansionReserve { get; set; }
        public CompanionReserve CompanionReserve { get; set; }
    }
}
