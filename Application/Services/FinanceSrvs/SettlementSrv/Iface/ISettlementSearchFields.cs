using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementSrv.Iface
{
    public interface ISettlementSearchFields
    {
        public long? ProviderId { get; set; }
        public Entities.Entities.SettlementKindEnum? Kind { get; set; }
        public long? StoreId { get; set; }
        public long? CompanionId { get; set; }
    }
}
