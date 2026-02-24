using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementCompanionSrv.Dto
{
    public class SettlementCompanionDto : Id_FieldDto
    {
        public long? CompanionReserveId { get; set; }
        public long? PansionReserveId { get; set; }
        public long SettlementId { get; set; }
    }
}
