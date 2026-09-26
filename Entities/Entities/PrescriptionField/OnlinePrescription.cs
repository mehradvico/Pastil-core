using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities.PrescriptionField
{
    // نسخه‌ی پزشک برای یک رزرو آنلاین (CompanionReserve) یا یک خرید مشاوره (ConsultationPurchase): متن و/یا تصویر.
    // برای هر هدف فقط یک نسخه وجود دارد (ایندکس یکتا)؛ نماینده همان را ویرایش می‌کند (حین تماس هم ذخیره‌ی خودکار).
    // دقیقاً یکی از CompanionReserveId / ConsultationPurchaseId پر است.
    public class OnlinePrescription : Id_Field
    {
        public long? CompanionReserveId { get; set; }
        public long? ConsultationPurchaseId { get; set; }
        // نمایندهای که نسخه را نوشته (پزشک/همکار)
        public long AuthorUserId { get; set; }
        public string Text { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool Deleted { get; set; }

        public CompanionReserve CompanionReserve { get; set; }
        public ConsultationPurchase ConsultationPurchase { get; set; }
        public User AuthorUser { get; set; }
        public ICollection<OnlinePrescriptionPicture> Pictures { get; set; }
    }

    public class OnlinePrescriptionPicture : Id_Field
    {
        public long OnlinePrescriptionId { get; set; }
        public long PictureId { get; set; }
        public int SortOrder { get; set; }
        public OnlinePrescription OnlinePrescription { get; set; }
        public Picture Picture { get; set; }
    }
}
