using Application.Common.Dto.Field;
using Application.Services.Accounting.UserPetSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistancePackageSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceUserSrv.Dto;
using Application.Services.Dto;
using Application.Services.Order.AddressSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.CompanionSrvs.CompanionReserveSrv.Dto
{
    public class CompanionReserveAdminVDto : Id_FieldDto
    {
        public long BookerId { get; set; }

        public double PrePaymentPrice { get; set; }
        public double OperatorFinalPrice { get; set; }
        public double OperatorStuffPrice { get; set; }
        public double OperatorWagesPrice { get; set; }
        public double PaymentPrice { get; set; }
        public double PackagePrice { get; set; }
        public long? AddressId { get; set; }
        public long CompanionAssistanceId { get; set; }
        public long CompanionAssistanceTimeId { get; set; }
        public long? CompanionAssistanceUserId { get; set; }
        public bool? IsFemale { get; set; }
        public string BookerDetail { get; set; }
        public string AssistanceDetail { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DoDate { get; set; }
        public DateTime? DoneDate { get; set; }
        public bool IsReserved { get; set; }
        public bool IsCancel { get; set; }
        public string CancelDetail { get; set; }
        public long StateId { get; set; }
        public long CompanionAssistanceTypeId { get; set; }
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
        public UserMinVDto Booker { get; set; }
        public CompanionAssistanceVDto CompanionAssistance { get; set; }
        public CompanionAssistanceTimeVDto CompanionAssistanceTime { get; set; }
        public CompanionAssistanceUserVDto CompanionAssistanceUser { get; set; }
        public CodeVDto State { get; set; }
        public CodeVDto OperatorState { get; set; }
        public CodeVDto CompanionAssistanceType { get; set; }
        public AddressVDto Address { get; set; }

        public List<UserPetVDto> UserPets { get; set; }
        public List<CompanionAssistancePackageVDto> CompanionAssistancePackages { get; set; }
    }
}
