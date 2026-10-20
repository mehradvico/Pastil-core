using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.StateSrv.Dto;
using AutoMapper;
using Entities.Entities.GroomerField;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.GroomerSrvs.GroomerSrv.Dto
{
    /// <summary>ورودی ساخت/ویرایش آرایشگاه (فیلدهای مدیریتی مثل تأیید و کمیسیون در DTOهای جدا هستند).</summary>
    public class GroomerDto : Name_FieldDto
    {
        public long? ProviderId { get; set; }
        /// <summary>اگر خالی باشد مالک = کاربر ارائه‌دهنده.</summary>
        public long? OwnerId { get; set; }
        public bool IsPersonal { get; set; }
        public long? PictureId { get; set; }
        public long? BackgroundPictureId { get; set; }
        public long? IconId { get; set; }
        public string Phone { get; set; }
        public string AddressValue { get; set; }
        public long StateId { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public PointDto Location { get; set; }

        public string Summary { get; set; }
        public string Description { get; set; }
        public string Regulations { get; set; }
        public int? EstablishedYear { get; set; }
        public string LicenseNumber { get; set; }
        public string InstagramUrl { get; set; }
        public string WebsiteUrl { get; set; }
        public bool ServiceAtHome { get; set; }
        public bool ServiceInSalon { get; set; }
        public bool PickupAvailable { get; set; }
        public double PickupFee { get; set; }
        public bool HasParking { get; set; }
        public bool HasWaitingRoom { get; set; }
        public bool HasCamera { get; set; }
        public int MaxConcurrentPets { get; set; }

        public string SeoH1 { get; set; }
        public string SeoMinDescription { get; set; }
        public string SeoDescription { get; set; }
        public string SeoTitle { get; set; }
        public string SeoPictureAlt { get; set; }
        public string SeoUrlText { get; set; }
        public bool SeoNoIndex { get; set; }
        public bool SeoNoFollow { get; set; }
        public string SeoCanonical { get; set; }
    }

    public class GroomerActiveDto : Id_FieldDto
    {
        public bool Active { get; set; }
    }

    public class GroomerApproveDto : Id_FieldDto
    {
        public bool Approve { get; set; }
        public string ApprovalValue { get; set; }
    }

    /// <summary>فیلدهایی که فقط ادمین تغییر می‌دهد.</summary>
    public class GroomerAdminSettingsDto : Id_FieldDto
    {
        public bool Suggested { get; set; }
        public bool LicenseApproved { get; set; }
        /// <summary>درصد کمیسیون پیش‌فرض (۰ تا ۱۰۰).</summary>
        public decimal CommissionPercent { get; set; }
    }

    public class GroomerOwnerVDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
    }

    public class GroomerProviderVDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
    }

    /// <summary>خلاصه‌ی آرایشگاه برای لیست‌ها.</summary>
    public class GroomerMinVDto : Name_FieldDto
    {
        public long? ProviderId { get; set; }
        public long OwnerId { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public bool ShowToSite { get; set; }
        public bool Suggested { get; set; }
        public string Summary { get; set; }
        public string Phone { get; set; }
        public string AddressValue { get; set; }
        public long CityId { get; set; }
        public int CommentCount { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public int PortfolioCount { get; set; }
        public int PortfolioLikeCount { get; set; }
        public bool ServiceAtHome { get; set; }
        public bool ServiceInSalon { get; set; }
        public DateTime? GoldAccountDate { get; set; }
        public DateTime? SilverAccountDate { get; set; }
        public PictureVDto Picture { get; set; }
        public PictureVDto Icon { get; set; }
        public CityVDto City { get; set; }
    }

    public class GroomerVDto : GroomerMinVDto
    {
        public string ActivationValue { get; set; }
        public long? LegacyCompanionId { get; set; }
        public bool IsPersonal { get; set; }
        public long? StateId { get; set; }
        public long? NeighborhoodId { get; set; }
        public long? PictureId { get; set; }
        public long? BackgroundPictureId { get; set; }
        public long? IconId { get; set; }
        public PointDto Location { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public DateTime CreateDate { get; set; }
        public string Description { get; set; }
        public string Regulations { get; set; }
        public int? EstablishedYear { get; set; }
        public string LicenseNumber { get; set; }
        public bool LicenseApproved { get; set; }
        public string InstagramUrl { get; set; }
        public string WebsiteUrl { get; set; }
        public bool PickupAvailable { get; set; }
        public double PickupFee { get; set; }
        public bool HasParking { get; set; }
        public bool HasWaitingRoom { get; set; }
        public bool HasCamera { get; set; }
        public int MaxConcurrentPets { get; set; }
        public int StaffCount { get; set; }
        public decimal CommissionPercent { get; set; }
        public string SeoH1 { get; set; }
        public string SeoMinDescription { get; set; }
        public string SeoDescription { get; set; }
        public string SeoTitle { get; set; }
        public string SeoPictureAlt { get; set; }
        public string SeoUrlText { get; set; }
        public bool SeoNoIndex { get; set; }
        public bool SeoNoFollow { get; set; }
        public string SeoCanonical { get; set; }
        public GroomerOwnerVDto Owner { get; set; }
        public GroomerProviderVDto Provider { get; set; }
        public StateVDto State { get; set; }
        public PictureVDto BackgroundPicture { get; set; }
        public List<long> PetIds { get; set; } = new List<long>();
    }

    public class GroomerInputDto : BaseInputDto
    {
        public long? ProviderId { get; set; }
        public long? OwnerId { get; set; }
        public bool? Approve { get; set; }
        public bool? Suggested { get; set; }
        public bool? ShowToSite { get; set; }
        public long? StateId { get; set; }
        public long? CityId { get; set; }
        public List<long> NeighborhoodIds { get; set; }
        public long? PetId { get; set; }
        public bool? ServiceAtHome { get; set; }
        public bool? ServiceInSalon { get; set; }
        public bool? PickupAvailable { get; set; }
        public long? AssistanceId { get; set; }
    }

    public class GroomerSearchDto : BaseSearchDto<Groomer, GroomerMinVDto>
    {
        public GroomerSearchDto(GroomerInputDto dto, IQueryable<Groomer> list, IMapper mapper) : base(dto, list, mapper)
        {
            ProviderId = dto.ProviderId;
            OwnerId = dto.OwnerId;
            Approve = dto.Approve;
            Suggested = dto.Suggested;
            ShowToSite = dto.ShowToSite;
            StateId = dto.StateId;
            CityId = dto.CityId;
            PetId = dto.PetId;
        }

        public long? ProviderId { get; set; }
        public long? OwnerId { get; set; }
        public bool? Approve { get; set; }
        public bool? Suggested { get; set; }
        public bool? ShowToSite { get; set; }
        public long? StateId { get; set; }
        public long? CityId { get; set; }
        public long? PetId { get; set; }
    }

    public class GroomerSiteMapDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>نتیجه‌ی جستجوی سراسری (Global Search).</summary>
    public class SearchGroomerDto : Name_FieldDto
    {
        public string Summary { get; set; }
        public string AddressValue { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public PictureVDto Picture { get; set; }
    }
}
