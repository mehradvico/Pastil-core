using Application.Common.Dto.Field;
using Application.Services.Accounting.UserPetSrv.Dto;
using Application.Services.Dto;
using Application.Services.Order.RebateSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using Entities.Entities;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionReserveSrv.Dto
{
    public class PansionReserveVDto : Id_FieldDto
    {
        // سفر پت‌رسانِ متصل (اگر بود) و مبلغ کل قابل‌مشاهده (رزرو + پت‌رسان)؛ پرداختشان کاملاً جداست
        public Application.Services.TripSrv.TripSrv.Dto.LinkedPetResanTripVDto PetResanTrip { get; set; }
        public double TotalPrice { get; set; }
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

        // تأیید مرکز: ۰ نیاز نیست، ۱ منتظر پاسخ مرکز، ۲ تأیید، ۳ رد، ۴ بی‌پاسخ ماند (لغو خودکار)
        public int OwnerDecision { get; set; }
        public DateTime? OwnerDecisionDate { get; set; }
        public string OwnerDecisionReason { get; set; }
        public DateTime? OwnerApprovalDeadline { get; set; }

        public double CompanionShare { get; set; }
        public double SiteShare { get; set; }

        public PansionMinVDto Pansion { get; set; }
        public UserPetVDto UserPet { get; set; }
        public CodeVDto Status { get; set; }
        public UserMinVDto Booker { get; set; }
        public RebateVDto Rebate { get; set; }
    }
}
