using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.SettlementSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementSrv.Dto
{
    public class SettlementSearchDto : BaseSearchDto<Settlement, SettlementVDto>, ISettlementSearchFields
    {
        public SettlementSearchDto(SettlementInputDto dto, IQueryable<Settlement> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.StoreId = dto.StoreId;
            this.CompanionId = dto.CompanionId;
        }
        public long? StoreId { get; set; }
        public long? CompanionId { get; set; }
    }
}
