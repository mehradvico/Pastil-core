using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class CompanionReserve : Id_Field
    {
        public long BookerId { get; set; }
        public double PrePaymentPrice { get; set; }
        public double OperatorFinalPrice { get; set; }
        public double OperatorStuffPrice { get; set; }
        public double OperatorWagesPrice { get; set; }
        public bool FromWallet { get; set; }
        public double WalletPrice { get; set; }
        public double PaymentPrice { get; set; }
        public double PackagePrice { get; set; }
        public long? AddressId { get; set; }
        public long CompanionAssistanceId { get; set; }
        public long CompanionAssistanceTypeId { get; set; }
        public long? CompanionAssistanceTimeId { get; set; }
        public long? CompanionAssistanceUserId { get; set; }
        public bool? IsFemale { get; set; }
        public string BookerDetail { get; set; }
        public string AssistanceDetail { get; set; }
        public bool IsReserved { get; set; }
        public bool IsCancel { get; set; }
        public string CancelDetail { get; set; }
        public long StateId { get; set; }
        public DateTime DoDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DoneDate { get; set; }
        public DateTime? CancelDate { get; set; }


        public long OperatorStateId { get; set; }
        public DateTime? OperatorChangeStateDate { get; set; }
        public string OperatorDetail { get; set; }
        public bool? UserResponse { get; set; }


        public double Discount { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }

        public double CompanionShare { get; set; }
        public double SiteShare { get; set; }

        public User Booker { get; set; }
        public Code CompanionAssistanceType { get; set; }
        public CompanionAssistance CompanionAssistance { get; set; }
        public CompanionAssistanceTime CompanionAssistanceTime { get; set; }
        public CompanionAssistanceUser CompanionAssistanceUser { get; set; }
        public Code State { get; set; }
        public Code OperatorState { get; set; }
        public Address Address { get; set; }
        public Rebate Rebate { get; set; }
        public Wallet Wallet { get; set; }
        public ICollection<CompanionAssistancePackage> CompanionAssistancePackages { get; set; }
        public ICollection<UserPet> UserPets { get; set; }
    }
}
