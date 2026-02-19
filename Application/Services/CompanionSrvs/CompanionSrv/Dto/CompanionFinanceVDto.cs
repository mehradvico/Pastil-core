using Application.Common.Dto.Field;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Application.Services.Dto;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionSrv.Dto
{
    public class CompanionFinanceVDto : Name_FieldDto
    {
        public long OwnerId { get; set; }
        public long? PictureId { get; set; }
        public string Phone { get; set; }
        public bool HasCommission { get; set; }
        public int TotalItemsCount { get; set; } 
        public int ItemsWithCommissionCount { get; set; }
        public UserMinVDto Owner { get; set; }

    }
}
