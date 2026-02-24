using Application.Common.Dto.Field;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementStoreSrv.Dto
{
    public class SettlementStoreVDto : Id_FieldDto
    {
        public string ProductOrderId { get; set; }
        public long SettlementId { get; set; }

        public ProductOrderVDto ProductOrder { get; set; }
        public SettlementVDto Settlement { get; set; }
    }
}
