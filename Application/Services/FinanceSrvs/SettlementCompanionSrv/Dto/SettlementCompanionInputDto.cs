using Application.Common.Dto.Input;
using Application.Services.FinanceSrvs.SettlementCompanionSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementCompanionSrv.Dto
{
    public class SettlementCompanionInputDto : BaseInputDto, ISettlementCompanionSearchFields
    {
        public long? SettlementId { get; set; }
    }
}
