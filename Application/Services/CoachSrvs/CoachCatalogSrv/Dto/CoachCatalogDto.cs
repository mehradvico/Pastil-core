using Application.Services.Filing.PictureSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.CoachSrvs.CoachCatalogSrv.Dto
{
    public static class CoachPetSizes
    {
        public const string Small = "Small";
        public const string Medium = "Medium";
        public const string Large = "Large";
        public static readonly string[] All = { Small, Medium, Large };
    }

    public class CoachServiceInputDto
    {
        public long Id { get; set; }
        public long CoachId { get; set; }
        /// <summary>خدمت از کاتالوگ سایت (Assistance)؛ بعد از ساخت قابل تغییر نیست.</summary>
        public long AssistanceId { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; }
    }

    public class CoachServiceApproveDto
    {
        public long Id { get; set; }
        public bool Approve { get; set; }
        public string ApprovalValue { get; set; }
        /// <summary>درصد کمیسیون سایت از این خدمت (۰ تا ۱۰۰)؛ خالی = بدون تغییر.</summary>
        public decimal? CommissionPercent { get; set; }
    }

    /// <summary>قیمت و پیش‌پرداخت یک بسته برای یک نحوه‌ی ارائه (Mode: ۳۷ آنلاین، ۳۸ حضوری در مربی، ۳۹ در محل مشتری).</summary>
    public class CoachPackageTypeDto
    {
        public int Mode { get; set; }
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
        /// <summary>مدت اقامت/دوره به روز (اختیاری).</summary>
        public int? StayDays { get; set; }
        /// <summary>تعداد جلسات (اختیاری).</summary>
        public int? SessionCount { get; set; }
    }

    public class CoachPackageTypeVDto : CoachPackageTypeDto
    {
        public long Id { get; set; }
        public string ModeName { get; set; }
    }

    public class CoachServicePackageInputDto
    {
        public long Id { get; set; }
        public long CoachServiceId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
        public long? PictureId { get; set; }
        public string Discription { get; set; }
        /// <summary>Small/Medium/Large؛ خالی = همه‌ی سایزها.</summary>
        public string PetSize { get; set; }
        /// <summary>قیمت به‌ازای نحوه‌ی ارائه. اگر خالی باشد بسته همه‌ی حالت‌ها را با Price/PrePaymentPrice ارائه می‌دهد؛ اگر پر باشد فقط حالت‌های لیست‌شده (Price/PrePaymentPrice بسته = ارزان‌ترین حالت).</summary>
        public List<CoachPackageTypeDto> Types { get; set; } = new List<CoachPackageTypeDto>();
    }

    public class CoachServicePackageVDto
    {
        public long Id { get; set; }
        public long CoachServiceId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
        public long? PictureId { get; set; }
        public PictureVDto Picture { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public string Discription { get; set; }
        public string PetSize { get; set; }
        public List<CoachPackageTypeVDto> Types { get; set; } = new List<CoachPackageTypeVDto>();
    }

    public class CoachServiceVDto
    {
        public long Id { get; set; }
        public long CoachId { get; set; }
        public long AssistanceId { get; set; }
        public string AssistanceName { get; set; }
        public PictureVDto AssistancePicture { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public string ActivationValue { get; set; }
        public decimal CommissionPercent { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; }
        public List<CoachServicePackageVDto> Packages { get; set; } = new List<CoachServicePackageVDto>();
        public List<long> UserIds { get; set; } = new List<long>();
    }
}
