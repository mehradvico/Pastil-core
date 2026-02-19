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
    public class FinanceSearchDto
    {
        public List<StoreFinanceVDto> Stores { get; set; } = new();
        public List<CompanionFinanceVDto> Companions { get; set; } = new();
    }
}
