using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementStoreSrv.Dto
{
    public class SettlementStoreDto : Id_FieldDto
    {
        public string ProductOrderId { get; set; }
        public long SettlementId { get; set; }
    }
}
