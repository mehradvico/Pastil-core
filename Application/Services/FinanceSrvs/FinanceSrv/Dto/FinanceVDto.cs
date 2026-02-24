using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceSrv.Dto
{
    public class FinanceVDto
    {
        public List<StoreFinanceVDto> Stores { get; set; }
        public List<CompanionFinanceVDto> Companions { get; set; }
    }
}
