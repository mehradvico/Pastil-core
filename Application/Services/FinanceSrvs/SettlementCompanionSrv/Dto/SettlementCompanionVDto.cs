using Application.Common.Dto.Field;
using Application.Services.CompanionSrv.CompanionReserveSrv.Dto;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Application.Services.PansionSrvs.PansionReserveSrv.Dto;
using Entities.Entities;
using Entities.Entities.PansionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementCompanionSrv.Dto
{
    public class SettlementCompanionVDto : Id_FieldDto
    {
        public long? CompanionReserveId { get; set; }
        public long? PansionReserveId { get; set; }
        public long SettlementId { get; set; }

        public SettlementVDto Settlement { get; set; }
        public PansionReserveVDto PansionReserve { get; set; }
        public CompanionReserveVDto CompanionReserve { get; set; }
    }
}
