using Application.Common.Dto.Field;
using Application.Services.CompanionSrvs.AssistanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;

namespace Application.Services.CompanionSrv.CompanionAssistanceSrv.Dto
{
    public class CompanionAssistanceVDto : Id_FieldDto
    {
        public long CompanionId { get; set; }
        public long AssistanceId { get; set; }
        public long CompanionTypeId { get; set; }
        public bool IsSinglePackage { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public string ActivationValue { get; set; }
        public decimal CommissionPercent { get; set; }

        public CompanionVDto Companion { get; set; }
        public AssistanceVDto Assistance { get; set; }
        public CodeVDto CompanionType { get; set; }
        public List<CodeVDto> Codes { get; set; }
    }
}
