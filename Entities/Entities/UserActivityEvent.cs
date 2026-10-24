using Entities.Entities.CommonField;
using System;

namespace Entities.Entities
{
    /// <summary>
    /// رویداد رفتاری کاربر در اپ (بازدید صفحه، کلیک، شروع نشست، ضربان فعالیت) برای «آنالیز کاربر» پنل.
    /// کلاینت‌ها رویدادها را batch می‌فرستند؛ زمان واقعی رویداد در CreateDate (UTC) و SessionId شناسه‌ی نشست سمت کلاینت است.
    /// </summary>
    public class UserActivityEvent : Id_Field
    {
        public long UserId { get; set; }
        public Guid? SessionId { get; set; }
        /// <summary>screen | click | session_start | heartbeat</summary>
        public string Type { get; set; }
        /// <summary>مسیر/نام صفحه، مثل /shop یا /pet/12</summary>
        public string Screen { get; set; }
        /// <summary>برچسب المان کلیک‌شده (مثل «افزودن به سبد»)</summary>
        public string Target { get; set; }
        /// <summary>web | android | ios | pwa ...</summary>
        public string Platform { get; set; }
        /// <summary>مدت حضور در صفحه/نشست (ثانیه) وقتی کلاینت اندازه‌گیری کرده</summary>
        public int? DurationSeconds { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
