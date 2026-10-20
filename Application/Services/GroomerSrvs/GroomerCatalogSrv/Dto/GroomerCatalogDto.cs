using Application.Services.Filing.PictureSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.GroomerSrvs.GroomerCatalogSrv.Dto
{
    public static class GroomerPetSizes
    {
        public const string Small = "Small";
        public const string Medium = "Medium";
        public const string Large = "Large";
        public static readonly string[] All = { Small, Medium, Large };
    }

    public class GroomerServiceInputDto
    {
        public long Id { get; set; }
        public long GroomerId { get; set; }
        /// <summary>خدمت از کاتالوگ سایت (Assistance)؛ بعد از ساخت قابل تغییر نیست.</summary>
        public long AssistanceId { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; }
    }

    public class GroomerServiceApproveDto
    {
        public long Id { get; set; }
        public bool Approve { get; set; }
        public string ApprovalValue { get; set; }
        /// <summary>درصد کمیسیون سایت از این خدمت (۰ تا ۱۰۰)؛ خالی = بدون تغییر.</summary>
        public decimal? CommissionPercent { get; set; }
    }

    public class GroomerServicePackageInputDto
    {
        public long Id { get; set; }
        public long GroomerServiceId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
        public long? PictureId { get; set; }
        public string Discription { get; set; }
        /// <summary>Small/Medium/Large؛ خالی = همه‌ی سایزها.</summary>
        public string PetSize { get; set; }
    }

    public class GroomerServicePackageVDto
    {
        public long Id { get; set; }
        public long GroomerServiceId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double PrePaymentPrice { get; set; }
        public long? PictureId { get; set; }
        public PictureVDto Picture { get; set; }
        public bool Active { get; set; }
        public string ActivationValue { get; set; }
        public string Discription { get; set; }
        public string PetSize { get; set; }
    }

    public class GroomerServiceVDto
    {
        public long Id { get; set; }
        public long GroomerId { get; set; }
        public long AssistanceId { get; set; }
        public string AssistanceName { get; set; }
        public PictureVDto AssistancePicture { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public string ActivationValue { get; set; }
        public decimal CommissionPercent { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; }
        public List<GroomerServicePackageVDto> Packages { get; set; } = new List<GroomerServicePackageVDto>();
        public List<long> UserIds { get; set; } = new List<long>();
    }
}
