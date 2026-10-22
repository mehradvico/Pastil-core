using Entities.Entities.CommonField;
using System;

namespace Entities.Entities
{
    /// <summary>
    /// کلید Idempotency-Key ثبت‌های ایجادی (رزرو، آدرس، ...) در دیتابیس تا بین چند replica و بعد از restart هم پایدار باشد.
    /// Key یکتا است (کاربر + روش + مسیر + کلید کلاینت)؛ ردیف تا ExpireDate نگه داشته می‌شود و Completed=true یعنی ResponseJson همان پاسخ موفق اول است.
    /// </summary>
    public class IdempotencyRecord : Id_Field
    {
        public string Key { get; set; }
        public long UserId { get; set; }
        public bool Completed { get; set; }
        public string ResponseJson { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}
