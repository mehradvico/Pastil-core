using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;

namespace Application.Services.CompanionSrv.CompanionReserveSrv.Dto
{
    public class CompanionReserveDto : Id_FieldDto
    {
        public string ReserveCode { get; set; }
        public long BookerId { get; set; }
        public List<long> UserPetIds { get; set; }
        public DateTime DoDate { get; set; }
        public DateTime? DoneDate { get; set; }

        public double PrePaymentPrice { get; set; }
        public bool FromWallet { get; set; }
        public double WalletPrice { get; set; }
        public double PaymentPrice { get; set; }
        public double PackagePrice { get; set; }
        public long? AddressId { get; set; }
        public long CompanionAssistanceId { get; set; }
        public long? CompanionAssistanceTimeId { get; set; }
        public long? CompanionAssistanceUserId { get; set; }
        public bool? IsFemale { get; set; }
        public string BookerDetail { get; set; }
        public string AssistanceDetail { get; set; }
        public long StateId { get; set; }
        public long CompanionAssistanceTypeId { get; set; }
        public bool IsReserved { get; set; }
        public bool IsCancel { get; set; }
        public string CancelDetail { get; set; }
        public long OperatorStateId { get; set; }
        public string RebateCode { get; set; }

        public List<long> CompanionAssistancePackagesIds { get; set; }
    }
}
