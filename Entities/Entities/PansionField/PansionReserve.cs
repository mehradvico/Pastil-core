using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.PansionField
{
    public class PansionReserve : Id_Field
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
        public int HourCount { get; set; }
        public int DayCount { get; set; }

        public double CompanionShare { get; set; }
        public double SiteShare { get; set; }
        public bool Permitted { get; set; }
        public Pansion Pansion { get; set; }
        public UserPet UserPet { get; set; }
        public Code Status { get; set; }
        public Rebate Rebate { get; set; }
        public Wallet Wallet { get; set; }
        public User Booker { get; set; }

    }
}
