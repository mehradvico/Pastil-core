using Application.Services.Filing.PictureSrv.Dto;

namespace Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto
{
    // یک پکیج مشاوره‌ی نام‌دار. Id=0 در ورودی یعنی «ایجاد»؛ Id>0 یعنی «ویرایش».
    public class ConsultationPackageItemDto
    {
        public long Id { get; set; }
        // مقدار OnlineSessionChannelEnum (۱ چت، ۲ تماس درون‌برنامه، ۳ تماس تصویری، ۴ تماس تلفنی)
        public int ChannelId { get; set; }
        // یکی از مدت‌های ثابت ConsultationRules.AllowedDurations
        public int DurationMinutes { get; set; }
        public double Price { get; set; }
        public bool Active { get; set; }
        // نام پکیج (الزامی، ۲ تا ۱۰۰ کاراکتر)، توضیح اختیاری (تا ۵۰۰ کاراکتر)
        public string Name { get; set; }
        public string Description { get; set; }
        // شناسه‌ی تصویر آپلود‌شده (اختیاری)
        public long? PictureId { get; set; }
        // فقط در خروجی
        public PictureVDto Picture { get; set; }
        public int SortOrder { get; set; }
    }

    // نمای عمومی برای کاربر: فقط پکیج‌های فعال با قیمت مثبت
    public class ConsultationPackagePublicVDto
    {
        public long Id { get; set; }
        public long CompanionId { get; set; }
        public int ChannelId { get; set; }
        public int DurationMinutes { get; set; }
        public double Price { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PictureVDto Picture { get; set; }
        public int SortOrder { get; set; }
    }
}
