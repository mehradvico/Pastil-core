using Application.Common.Dto.Field;
using System;

namespace Application.Services.SchoolSrvs.SchoolReserveSrv.Dto
{
    public class SchoolReserveDto : Id_FieldDto
    {
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
        public double CompanionShare { get; set; }
        public double SiteShare { get; set; }
    }
}
