using Application.Common.Dto.Input;
using Application.Services.Filing.PictureSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.Accounting.PetMicrochipSrv.Dto
{
    // ---------- عمومی (سایت/اپ، بخش «ارتباط با ما»)
    public class PetMicrochipSearchDto
    {
        /// <summary>کد میکروچیپ (۹ تا ۱۵ رقم؛ ارقام فارسی، فاصله و خط تیره پذیرفته و نرمال می‌شود)</summary>
        public string MicrochipCode { get; set; }
    }

    /// <summary>پروفایل عمومی پت. مشخصات مالک (نام/موبایل/ایمیل/آدرس/شناسه) و اطلاعات درمانی (بیماری/دارو) عمداً وجود ندارد.</summary>
    public class PetMicrochipPublicPetVDto
    {
        public string Name { get; set; }
        public string PetName { get; set; }
        public string BreedName { get; set; }
        public string Breed2Name { get; set; }
        public bool IsMixBreed { get; set; }
        public bool IsMale { get; set; }
        public bool IsSterile { get; set; }
        public int? AgeMonths { get; set; }
        public string Size { get; set; }
        public string Weight { get; set; }
        public PictureVDto Picture { get; set; }
        public List<PictureVDto> Pictures { get; set; } = new List<PictureVDto>();
    }

    public class PetMicrochipSearchResultVDto
    {
        public bool Found { get; set; }
        public string MicrochipCode { get; set; }
        /// <summary>فقط وقتی Found=true؛ برای ثبت «درخواست پیگیری» (۲۴ ساعت اعتبار)</summary>
        public string Token { get; set; }
        public DateTime? TokenExpireDate { get; set; }
        public PetMicrochipPublicPetVDto Pet { get; set; }
    }

    public class PetMicrochipFollowUpDto
    {
        public string Token { get; set; }
        /// <summary>اگر کاربر لاگین است و خالی بفرستد از پروفایل خودش پر می‌شود</summary>
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }

    public class PetMicrochipFollowUpResultVDto
    {
        public long RequestId { get; set; }
        public DateTime FollowUpDate { get; set; }
    }

    // ---------- ادمین
    public class PetMicrochipRequestInputDto : BaseInputDto
    {
        /// <summary>1..5 (PetMicrochipRequestStatusEnum). خالی = فقط درخواست‌های پیگیری (۲ تا ۵)</summary>
        public int? Status { get; set; }
        /// <summary>true = جستجوهای بدون درخواست پیگیری هم بیاید</summary>
        public bool IncludeSearches { get; set; }
        public string MicrochipCode { get; set; }
    }

    public class PetMicrochipRequestListItemVDto
    {
        public long Id { get; set; }
        public string MicrochipCode { get; set; }
        public int Status { get; set; }
        public bool Found { get; set; }
        public bool FollowUpRequested { get; set; }
        public string PetName { get; set; }
        public string RequesterName { get; set; }
        public string RequesterMobile { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? FollowUpDate { get; set; }
    }

    public class PetMicrochipRequestSearchVDto
    {
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<PetMicrochipRequestListItemVDto> List { get; set; } = new List<PetMicrochipRequestListItemVDto>();
    }

    /// <summary>مشخصات تماس مالک پت؛ فقط برای ادمین</summary>
    public class PetMicrochipOwnerVDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
    }

    public class PetMicrochipRequestDetailVDto
    {
        public long Id { get; set; }
        public string MicrochipCode { get; set; }
        public int Status { get; set; }
        public int MatchCount { get; set; }
        public DateTime CreateDate { get; set; }
        public string ClientIp { get; set; }
        public long? RequesterUserId { get; set; }
        public bool FollowUpRequested { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public string RequesterName { get; set; }
        public string RequesterMobile { get; set; }
        public string RequesterEmail { get; set; }
        public string Message { get; set; }
        public string AdminNote { get; set; }
        public DateTime? ClosedDate { get; set; }
        public long? FoundUserPetId { get; set; }
        public PetMicrochipPublicPetVDto Pet { get; set; }
        public PetMicrochipOwnerVDto Owner { get; set; }
        /// <summary>وضعیت‌هایی که از وضعیت فعلی مجازند</summary>
        public List<int> AllowedNextStatuses { get; set; } = new List<int>();
    }

    public class PetMicrochipRequestUpdateDto
    {
        public int Status { get; set; }
        public string AdminNote { get; set; }
    }
}
