using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.StateSrv.Dto;
using AutoMapper;
using Entities.Entities.ClinicField;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.ClinicSrvs.ClinicSrv.Dto
{
    /// <summary>ورودی ساخت/ویرایش کلینیک (فیلدهای مدیریتی مثل تأیید و کمیسیون در DTOهای جدا هستند).</summary>
    public class ClinicDto : Name_FieldDto
    {
        public long? ProviderId { get; set; }
        /// <summary>اگر خالی باشد مالک = کاربر ارائه‌دهنده.</summary>
        public long? OwnerId { get; set; }
        /// <summary>انواع کلینیک (کدهای ۴۰ کلینیک، ۴۲ آزمایشگاه، ۴۴ پرستار)؛ خالی = کلینیک (۴۰).</summary>
        public List<long> TypeIds { get; set; } = new List<long>();
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
        /// <summary>خدمات در محل مشتری (با پوشش ClinicZone).</summary>
        public bool ServiceAtHome { get; set; }
        /// <summary>خدمات حضوری در خود کلینیک.</summary>
        public bool ServiceInPerson { get; set; }
        /// <summary>خدمات آنلاین (مشاوره‌ی تصویری/چت).</summary>
        public bool ServiceOnline { get; set; }
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

    public class ClinicActiveDto : Id_FieldDto
    {
        public bool Active { get; set; }
    }

    public class ClinicApproveDto : Id_FieldDto
    {
        public bool Approve { get; set; }
        public string ApprovalValue { get; set; }
    }

    /// <summary>فیلدهایی که فقط ادمین تغییر می‌دهد.</summary>
    public class ClinicAdminSettingsDto : Id_FieldDto
    {
        public bool Suggested { get; set; }
        public bool LicenseApproved { get; set; }
        /// <summary>درصد کمیسیون پیش‌فرض (۰ تا ۱۰۰).</summary>
        public decimal CommissionPercent { get; set; }
    }

    public class ClinicOwnerVDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
    }

    public class ClinicProviderVDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
    }

    /// <summary>خلاصه‌ی کلینیک برای لیست‌ها.</summary>
    public class ClinicMinVDto : Name_FieldDto
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
        public bool ServiceAtHome { get; set; }
        public bool ServiceInPerson { get; set; }
        public bool ServiceOnline { get; set; }
        public DateTime? GoldAccountDate { get; set; }
        public DateTime? SilverAccountDate { get; set; }
        public PictureVDto Picture { get; set; }
        public PictureVDto Icon { get; set; }
        public CityVDto City { get; set; }
    }

    public class ClinicVDto : ClinicMinVDto
    {
        public string ActivationValue { get; set; }
        public long? LegacyCompanionId { get; set; }
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
        public ClinicOwnerVDto Owner { get; set; }
        public ClinicProviderVDto Provider { get; set; }
        public StateVDto State { get; set; }
        public PictureVDto BackgroundPicture { get; set; }
        public List<long> PetIds { get; set; } = new List<long>();
        public List<long> TypeIds { get; set; } = new List<long>();
    }

    public class ClinicInputDto : BaseInputDto
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
        public bool? ServiceInPerson { get; set; }
        public bool? ServiceOnline { get; set; }
        /// <summary>نوع کلینیک (۴۰ کلینیک، ۴۲ آزمایشگاه، ۴۴ پرستار).</summary>
        public long? TypeId { get; set; }
        public long? AssistanceId { get; set; }
    }

    public class ClinicSearchDto : BaseSearchDto<Clinic, ClinicMinVDto>
    {
        public ClinicSearchDto(ClinicInputDto dto, IQueryable<Clinic> list, IMapper mapper) : base(dto, list, mapper)
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

    public class ClinicSiteMapDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>نتیجه‌ی جستجوی سراسری (Global Search).</summary>
    public class SearchClinicDto : Name_FieldDto
    {
        public string Summary { get; set; }
        public string AddressValue { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public PictureVDto Picture { get; set; }
    }
}
