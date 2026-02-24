using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.SettlementStoreSrv.Dto;
using Application.Services.FinanceSrvs.SettlementStoreSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementStoreStoreSrv.Dto
{
    public class SettlementStoreSearchDto : BaseSearchDto<SettlementStore, SettlementStoreVDto>, ISettlementStoreSearchFields
    {
        public SettlementStoreSearchDto(SettlementStoreInputDto dto, IQueryable<SettlementStore> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.SettlementId = dto.SettlementId;
        }
        public long? SettlementId { get; set; }

    }
}
