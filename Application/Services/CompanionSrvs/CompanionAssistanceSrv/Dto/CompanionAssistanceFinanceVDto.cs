using Application.Common.Dto.Field;
using Application.Services.CompanionSrvs.AssistanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto
{
    public class CompanionAssistanceFinanceVDto : Id_FieldDto
    {
        public long CompanionId { get; set; }
        public long AssistanceId { get; set; }
        public decimal CommissionPercent { get; set; }
        public bool HasCommission { get; set; }
        public CompanionMinVDto Companion { get; set; }
        public AssistanceVDto Assistance { get; set; }

    }
}
