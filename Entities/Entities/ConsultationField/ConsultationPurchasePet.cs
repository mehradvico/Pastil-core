using Entities.Entities.CommonField;
using System;

namespace Entities.Entities
{
    // پت‌هایی که مشاوره‌ی آنلاین برایشان خریده شده (۲۰۲۶-۱۰-۲۷). مشتری برای پت‌هایش مشاوره می‌گیرد، نه برای خودش؛
    // نماینده‌ی کلینیک/مربی پروفایل و سابقه‌ی پزشکی همین پت‌ها را می‌بیند. خریدهای قبل از این تاریخ ردیفی ندارند.
    public class ConsultationPurchasePet : Id_Field
    {
        public long ConsultationPurchaseId { get; set; }
        public long UserPetId { get; set; }
        public DateTime CreateDate { get; set; }

        public ConsultationPurchase ConsultationPurchase { get; set; }
        public UserPet UserPet { get; set; }
    }
}
