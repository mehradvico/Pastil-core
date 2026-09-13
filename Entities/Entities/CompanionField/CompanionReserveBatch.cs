using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    // سبد رزرو: چند رزرو (هر کدام برای یک «خدمت» متفاوت) که همگی متعلق به یک نماینده/کلینیک هستند
    // و کاربر آن‌ها را با هم، در یک پرداخت واحد، نهایی می‌کند. انتخاب/تخصیص متخصص، لغو و مدیریت هر
    // رزرو همچنان جداگانه و دقیقاً مثل قبل انجام می‌شود؛ این موجودیت فقط سطح «پرداخت مشترک» را اضافه می‌کند.
    public class CompanionReserveBatch : Id_Field
    {
        public long UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public bool Deleted { get; set; }

        public User User { get; set; }
        public ICollection<CompanionReserve> CompanionReserves { get; set; }
    }
}
