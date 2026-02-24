using Application.Common.Dto.Field;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.FinanceSrvs.UserBankCardSrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementSrv.Dto
{
    public class SettlementVDto : Id_FieldDto
    {
        public long? StoreId { get; set; }
        public long? CompanionId { get; set; }
        public DateTime CreateDate { get; set; }
        public long UserBankCardId { get; set; }
        public string TrackingCode { get; set; }
        public double PaidPrice { get; set; }
        public long ItemCount { get; set; }

        public UserBankCardVDto UserBankCard { get; set; }
        public CompanionMinVDto Companion { get; set; }
        public StoreMinVDto Store { get; set; }
    }
}
