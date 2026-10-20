using Application.Services.Filing.PictureSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.ClinicSrvs.ClinicCatalogSrv.Dto
{
    public static class ClinicPetSizes
    {
        public const string Small = "Small";
        public const string Medium = "Medium";
        public const string Large = "Large";
        public static readonly string[] All = { Small, Medium, Large };
    }

    public class ClinicServiceInputDto
    {
        public long Id { get; set; }
        public long ClinicId { get; set; }
        /// <summary>خدمت از کاتالوگ سایت (Assistance)؛ بعد از ساخت قابل تغییر نیست.</summary>
        public long AssistanceId { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; }
    }

    public class ClinicServiceApproveDto
    {
        public long Id { get; set; }
        public bool Approve { get; set; }
        public string ApprovalValue { get; set; }
        /// <summary>درصد کمیسیون سایت از این خدمت (۰ تا ۱۰۰)؛ خالی = بدون تغییر.</summary>
        public decimal? CommissionPercent { get; set; }
    }

    /// <summary>قیمت و پیش‌پرداخت یک بسته برای یک نحوه‌ی ارائه (Mode: ۳۷ آنلاین، ۳۸ حضوری در کلینیک، ۳۹ در محل مشتری).</summary>
    public class ClinicPackageTypeDto
    {
        public int Mode { get; set; }
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
    }

    public class ClinicPackageTypeVDto : ClinicPackageTypeDto
    {
        public long Id { get; set; }
        public string ModeName { get; set; }
    }

    public class ClinicServicePackageInputDto
    {
        public long Id { get; set; }
        public long ClinicServiceId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
        public long? PictureId { get; set; }
        public string Discription { get; set; }
        /// <summary>Small/Medium/Large؛ خالی = همه‌ی سایزها.</summary>
        public string PetSize { get; set; }
        /// <summary>قیمت به‌ازای نحوه‌ی ارائه. اگر خالی باشد بسته همه‌ی حالت‌ها را با Price/PrePaymentPrice ارائه می‌دهد؛ اگر پر باشد فقط حالت‌های لیست‌شده (Price/PrePaymentPrice بسته = ارزان‌ترین حالت).</summary>
        public List<ClinicPackageTypeDto> Types { get; set; } = new List<ClinicPackageTypeDto>();
    }

    public class ClinicServicePackageVDto
    {
        public long Id { get; set; }
        public long ClinicServiceId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
        public long? PictureId { get; set; }
        public PictureVDto Picture { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public string Discription { get; set; }
        public string PetSize { get; set; }
        public List<ClinicPackageTypeVDto> Types { get; set; } = new List<ClinicPackageTypeVDto>();
    }

    public class ClinicServiceVDto
    {
        public long Id { get; set; }
        public long ClinicId { get; set; }
        public long AssistanceId { get; set; }
        public string AssistanceName { get; set; }
        public PictureVDto AssistancePicture { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public string ActivationValue { get; set; }
        public decimal CommissionPercent { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; }
        public List<ClinicServicePackageVDto> Packages { get; set; } = new List<ClinicServicePackageVDto>();
        public List<long> UserIds { get; set; } = new List<long>();
    }
}
