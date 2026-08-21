using Application.Common.Dto.Field;
using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionReserveSrv.Dto
{
    public class PansionReserveDto : Id_FieldDto
    {
        public string ReserveCode { get; set; }
        public long PansionId { get; set; }
        public long BookerId { get; set; }
        public long UserPetId { get; set; }

        public double Price { get; set; }
        public bool FromWallet { get; set; }
        public double WalletPrice { get; set; }
        public double PaymentPrice { get; set; }
        public bool IsReserved { get; set; }
        public bool IsCancel { get; set; }
        public string CancelDetail { get; set; }
        public string StartTime { get; set; } // School
        public string EndTime { get; set; }
        public DateTime? SchoolCreateDate { get; set; }
        public DateTime? FromDate { get; set; } // Pansion
        public DateTime? ToDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public double Discount { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        public long StatusId { get; set; }
    }
}
