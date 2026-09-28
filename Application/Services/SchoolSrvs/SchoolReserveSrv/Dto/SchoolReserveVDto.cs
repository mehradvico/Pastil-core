using Application.Common.Dto.Field;
using Application.Services.Accounting.UserPetSrv.Dto;
using Application.Services.Dto;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using System;

namespace Application.Services.SchoolSrvs.SchoolReserveSrv.Dto
{
    public class SchoolReserveVDto : Id_FieldDto
    {
        // سفر پت‌رسانِ متصل (اگر بود) و مبلغ کل قابل‌مشاهده (رزرو + پت‌رسان)؛ پرداختشان کاملاً جداست
        public Application.Services.TripSrv.TripSrv.Dto.LinkedPetResanTripVDto PetResanTrip { get; set; }
        public double TotalPrice { get; set; }
        public string ReserveCode { get; set; }
        public long SchoolCourseId { get; set; }
        public long BookerId { get; set; }
        public long UserPetId { get; set; }
        public double Price { get; set; }
        public bool FromWallet { get; set; }
        public double WalletPrice { get; set; }
        public double PaymentPrice { get; set; }
        public bool IsReserved { get; set; }
        public bool IsCancel { get; set; }
        public string CancelDetail { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        public int StatusId { get; set; }

        public SchoolCourseVDto SchoolCourse { get; set; }
        public UserPetVDto UserPet { get; set; }
        public UserMinVDto Booker { get; set; }
    }
}
