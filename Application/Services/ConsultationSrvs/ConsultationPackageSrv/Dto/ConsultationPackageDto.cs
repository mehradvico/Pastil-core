using System.Collections.Generic;

namespace Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto
{
    // یک خانه‌ی ماتریس پکیج مشاوره (کانال × مدت). Id=0 یعنی هنوز ذخیره نشده (مقدار پیش‌فرض: غیرفعال با قیمت ۰)
    public class ConsultationPackageItemDto
    {
        public long Id { get; set; }
        public int ChannelId { get; set; }
        public int DurationMinutes { get; set; }
        public double Price { get; set; }
        public bool Active { get; set; }
    }

    public class ConsultationPackageSaveDto
    {
        public List<ConsultationPackageItemDto> Items { get; set; } = new();
    }

    // نمای عمومی برای کاربر: فقط پکیج‌های فعال با قیمت مثبت
    public class ConsultationPackagePublicVDto
    {
        public long Id { get; set; }
        public long CompanionId { get; set; }
        public int ChannelId { get; set; }
        public int DurationMinutes { get; set; }
        public double Price { get; set; }
    }
}
