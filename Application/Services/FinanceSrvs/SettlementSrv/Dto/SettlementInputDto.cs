using Application.Common.Dto.Input;
using Application.Services.FinanceSrvs.SettlementSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementSrv.Dto
{
    public class SettlementInputDto : BaseInputDto, ISettlementSearchFields
    {
        public long? StoreId { get; set; }
        public long? CompanionId { get; set; }

    }
}
