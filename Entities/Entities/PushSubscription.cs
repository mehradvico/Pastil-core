using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class PushSubscription : Id_Field
    {
        public long? UserId { get; set; }

        public Guid? DeviceKey { get; set; }

        // پیش‌فرض WebPush (Application.Common.Enumerable.PushProviderEnum) — ردیف‌های قدیمی
        // همه Migration را با همین مقدار می‌گیرند تا رفتار وب‌اپ فعلی دست‌نخورده بماند.
        public long Provider { get; set; }

        // فقط برای Provider=WebPush
        public string Endpoint { get; set; }
        public string P256dh { get; set; }
        public string Auth { get; set; }

        // فقط برای Provider=Fcm (اپ فلاتر اندروید/iOS)
        public string FcmToken { get; set; }
        // "android" | "ios" | "windows" | "web" — فقط جهت گزارش‌گیری/عیب‌یابی، در تصمیم ارسال استفاده نمی‌شود
        public string Platform { get; set; }

        public string UserAgent { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastSeen { get; set; }
        public bool IsActive { get; set; }

        public User User { get; set; }
    }

}
