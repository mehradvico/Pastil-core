using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementSrv.Dto
{
    public class SettlementDto : Id_FieldDto
    {
        public long? StoreId { get; set; }
        public long? CompanionId { get; set; }
        public long UserBankCardId { get; set; }
        public string TrackingCode { get; set; }
    }
}
