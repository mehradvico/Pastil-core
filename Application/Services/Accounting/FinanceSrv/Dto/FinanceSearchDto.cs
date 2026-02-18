using Application.Services.Accounting.FinanceSrv.Dto;
using Application.Services.Accounting.FinanceSrv.Iface;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using AutoMapper;
using Entities.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.FinanceSrv.Dto
{
    public class FinanceSearchDto : IFinanceSearchFields
    {
        public FinanceSearchDto(
            FinanceInputDto dto,
            IQueryable<Store> stores,
            IQueryable<Companion> companions,
            IMapper mapper)
        {
            IsCompanion = dto.IsCompanion;

            Stores = stores.Select(s => mapper.Map<StoreFinanceVDto>(s)).ToList();
            Companions = companions.Select(c => mapper.Map<CompanionFinanceVDto>(c)).ToList();
        }

        public bool? IsCompanion { get; set; }

        public List<StoreFinanceVDto> Stores { get; set; } = new();
        public List<CompanionFinanceVDto> Companions { get; set; } = new();
    }
}
