using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionSrv.Dto
{
    public class CompanionFinanceDetailVDto : CompanionFinanceVDto
    {
        public new List<PansionFinanceVDto> Pansions { get; set; }
        public new List<CompanionAssistanceFinanceVDto> CompanionAssistances { get; set; }
    }
}
