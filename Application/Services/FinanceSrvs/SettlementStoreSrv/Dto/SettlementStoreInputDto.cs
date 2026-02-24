using Application.Common.Dto.Input;
using Application.Services.FinanceSrvs.SettlementStoreSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementStoreStoreSrv.Dto
{
    public class SettlementStoreInputDto : BaseInputDto, ISettlementStoreSearchFields
    {
        public long? SettlementId { get; set; }
    }
}
