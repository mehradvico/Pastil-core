using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.SchoolField
{
    // ثبت‌نام یک پت در یک دوره‌ی مدرسه. ظرفیت دوره (SchoolCourse.Capacity) در سرویس، در برابر
    // تعداد SchoolReserve های غیرلغوشده‌ی همان دوره چک می‌شود - نه اینجا.
    // StatusId: Application.Common.Enumerable.Code.SchoolReserveStatusEnum (مستقل از جدول Codes).
    public class SchoolReserve : Id_Field
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
        public double Discount { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        public int StatusId { get; set; }

        public double CompanionShare { get; set; }
        public double SiteShare { get; set; }

        public SchoolCourse SchoolCourse { get; set; }
        public UserPet UserPet { get; set; }
        public Rebate Rebate { get; set; }
        public User Booker { get; set; }
    }
}
