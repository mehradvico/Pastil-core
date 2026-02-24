using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class SettlementStore : Id_Field
    {
        public string ProductOrderId { get; set; }
        public long SettlementId { get; set; }

        public ProductOrder ProductOrder { get; set; }
        public Settlement Settlement { get; set; }
    }
}
