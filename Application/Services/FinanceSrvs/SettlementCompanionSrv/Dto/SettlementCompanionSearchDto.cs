using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.SettlementCompanionSrv.Dto;
using Application.Services.FinanceSrvs.SettlementCompanionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementCompanionSrv.Dto
{
    public class SettlementCompanionSearchDto : BaseSearchDto<SettlementCompanion, SettlementCompanionVDto>, ISettlementCompanionSearchFields
    {
        public SettlementCompanionSearchDto(SettlementCompanionInputDto dto, IQueryable<SettlementCompanion> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.SettlementId = dto.SettlementId;
        }
        public long? SettlementId { get; set; }

    }
}
