using Application.Services.Filing.PictureSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.CompanionSrvs.CompanionDayScheduleSrv.Dto
{
    // وضعیت یک آیتم در برنامه‌ی روز (ثابت‌های عددی؛ در پاسخ به‌صورت int می‌آیند)
    public static class DayItemStatus
    {
        public const int Upcoming = 1;   // هنوز نرسیده
        public const int Now = 2;        // الان داخل بازه‌ی زمانی یا مشاوره‌ی در جریان
        public const int Done = 3;       // انجام شده
        public const int Missed = 4;     // زمانش گذشته و انجام نشده
        public const int Cancelled = 5;  // لغو/بازپرداخت‌شده
    }

    public static class DayItemKinds
    {
        public const string Service = "Service";             // رزرو خدمت (CompanionReserve)
        public const string Consultation = "Consultation";   // مشاوره‌ی آنلاین رزروشده با ساعت (ConsultationPurchase)
    }

    // یک بازه‌ی ساعت کاری کلینیک در همان روز
    public class DayWorkingHourVDto
    {
        public long Id { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        // ظرفیت هم‌زمان ثبت‌شده برای این بازه
        public int Capacity { get; set; }
    }

    public class DayCustomerVDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public PictureVDto Picture { get; set; }
    }

    public class DayPetVDto
    {
        public long UserPetId { get; set; }
        public string Name { get; set; }
        // نوع حیوان (سگ، گربه، …)
        public string PetName { get; set; }
        public string BreedName { get; set; }
        public string Breed2Name { get; set; }
        public bool IsMixBreed { get; set; }
        public bool IsMale { get; set; }
        public bool IsSterile { get; set; }
        public DateTime Birthday { get; set; }
        // سن به ماه (۰ اگر تاریخ تولد نامعتبر)؛ متن «۲ سال و ۳ ماه» را کلاینت می‌سازد
        public int AgeMonths { get; set; }
        public string Size { get; set; }
        public string Weight { get; set; }
        // سابقه‌ی بیماری/دارو؛ برای آماده‌شدن نماینده قبل از نوبت
        public string SpecificDisease { get; set; }
        public string SpecificMedicine { get; set; }
        public string MicroChipCode { get; set; }
        public PictureVDto Picture { get; set; }
    }

    public class DayAssigneeVDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        // true = نماینده‌ی کاربر جاری
        public bool IsMe { get; set; }
    }

    public class DayAddressVDto
    {
        public string AddressValue { get; set; }
        public string Unit { get; set; }
        public string Floor { get; set; }
        public string CityName { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class DayPackageVDto
    {
        public string Name { get; set; }
        public string PetSize { get; set; }
    }

    public class DayScheduleItemVDto
    {
        // "Service" یا "Consultation"
        public string Kind { get; set; }
        // شناسه‌ی رزرو (Service) یا خرید مشاوره (Consultation)
        public long Id { get; set; }
        public string Code { get; set; }
        public DateTime Start { get; set; }
        // null اگر ساعت پایان مشخص نیست
        public DateTime? End { get; set; }
        // «HH:mm» برای نمایش مستقیم
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        // ۱ نرسیده، ۲ الان، ۳ انجام‌شده، ۴ از دست رفته، ۵ لغوشده (DayItemStatus)
        public int Status { get; set; }
        // نام خدمت یا نام پکیج مشاوره
        public string Title { get; set; }
        // نوع ارائه‌ی رزرو خدمت: مرکز/در محل/آنلاین (نام کد)
        public string ServiceModeName { get; set; }
        public string StateName { get; set; }
        public bool IsOnline { get; set; }
        public string OnlineMethodName { get; set; }
        public DayCustomerVDto Customer { get; set; }
        public List<DayPetVDto> Pets { get; set; } = new();
        public DayAssigneeVDto Assignee { get; set; }
        public List<DayPackageVDto> Packages { get; set; } = new();
        public DayAddressVDto Address { get; set; }
        // توضیح کاربر هنگام رزرو
        public string CustomerNote { get; set; }
        public string AssistanceDetail { get; set; }
        public string CancelDetail { get; set; }
        public double PaymentPrice { get; set; }
        // «پرداخت‌نشده»: هزینه‌ی نهایی ثبت شده ولی کاربر پرداخت نکرده
        public bool OperatorUnpaid { get; set; }
        public double OperatorUnpaidAmount { get; set; }

        // ---- فقط Consultation
        public int? ChannelId { get; set; }
        public int? DurationMinutes { get; set; }
        public int? ConsultationStatus { get; set; }
        public long? OnlineSessionId { get; set; }
        // true اگر همین الان می‌تواند «شروع» را بزند (از ۱۰ دقیقه قبل تا ساعت+۱۵ دقیقه)؛ شروع: POST /api/Companion/ConsultationSession/{id}/Start
        public bool CanStart { get; set; }
    }

    public class DayScheduleSummaryVDto
    {
        public int Total { get; set; }
        public int Upcoming { get; set; }
        public int Now { get; set; }
        public int Done { get; set; }
        public int Missed { get; set; }
        public int Cancelled { get; set; }
    }

    public class DayClinicVDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsOwner { get; set; }
    }

    public class CompanionDayScheduleVDto
    {
        public long CompanionId { get; set; }
        public string CompanionName { get; set; }
        // روز درخواستی (۰۰:۰۰)
        public DateTime Date { get; set; }
        // ساعت سرور؛ «الان» و وضعیت‌ها با آن حساب می‌شوند (کلینیک از ساعت دستگاه استفاده نکند)
        public DateTime ServerNow { get; set; }
        // نام انگلیسی روز هفته (Saturday…) و شماره‌ی روز در تقویم پروژه (۱ شنبه … ۷ جمعه)
        public string DayOfWeek { get; set; }
        public int WeekDayId { get; set; }
        // true اگر کاربر مالک کلینیک است (همه‌ی رزروها را می‌بیند)؛ false = فقط رزروهای تخصیص‌یافته به خودش
        public bool IsOwner { get; set; }
        // همه‌ی کلینیک‌هایی که کاربر مالک یا عضو فعال آن‌هاست (برای انتخابگر کلینیک وقتی بیش از یکی است)
        public List<DayClinicVDto> AvailableClinics { get; set; } = new();
        public List<DayWorkingHourVDto> WorkingHours { get; set; } = new();
        public DayScheduleSummaryVDto Summary { get; set; } = new();
        public List<DayScheduleItemVDto> Items { get; set; } = new();
    }
}
