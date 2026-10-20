using Application.Common.Dto.Input;
using System;
using System.Collections.Generic;

namespace Application.Services.AgencySrvs.AgencySrv.Dto
{
    /// <summary>نوع نمایندگی در لیست یکپارچه: فعلاً کلینیک/آرایشگاه/مربی (Companion) و فروشگاه (Store).</summary>
    public enum AgencyKindEnum
    {
        Companion = 1,
        Store = 2,
        Pansion = 3,
        School = 4,
        Groomer = 5,
        Clinic = 6,
        Coach = 7,
    }

    public class AgencyInputDto : BaseInputDto
    {
        /// <summary>خالی = هر دو نوع.</summary>
        public AgencyKindEnum? Kind { get; set; }
        public bool? Approved { get; set; }
        /// <summary>نقش ارائه‌دهنده‌ی نمایندگی (ProviderRole)؛ نوع نمایندگی از نقش ارائه‌دهنده می‌آید.</summary>
        public long? ProviderRoleId { get; set; }
        public long? ProviderId { get; set; }
        /// <summary>فقط نمایندگی‌ها (Companion)؛ با ست‌شدن، فروشگاه‌ها از نتیجه حذف می‌شوند.</summary>
        public bool? IsPersonal { get; set; }
        public bool? GoldAccount { get; set; }
        public bool? SilverAccount { get; set; }
    }

    public class AgencyNameVDto
    {
        public string Name { get; set; }
    }

    public class AgencyCityVDto
    {
        public string Name { get; set; }
        public AgencyNameVDto State { get; set; }
    }

    public class AgencyOwnerVDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
    }

    public class AgencyPictureVDto
    {
        public string Url { get; set; }
    }

    /// <summary>یک ردیف لیست یکپارچه‌ی نمایندگی‌ها (فروشگاه + کلینیک/آرایشگاه/مربی). فقط ستون‌های مشترک.</summary>
    public class AgencyVDto
    {
        /// <summary>1 = نمایندگی (Companion)، 2 = فروشگاه، 3 = پانسیون، 4 = مدرسه، 5 = آرایشگاه (Groomer)، 6 = کلینیک (Clinic)، 7 = مربی (Coach).</summary>
        public int Kind { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public long? ProviderId { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public string ActivationValue { get; set; }
        public bool IsPersonal { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public DateTime? GoldAccountDate { get; set; }
        public DateTime? SilverAccountDate { get; set; }
        public long OwnerId { get; set; }
        public AgencyOwnerVDto Owner { get; set; }
        public AgencyCityVDto City { get; set; }
        public AgencyNameVDto Neighborhood { get; set; }
        public AgencyPictureVDto Picture { get; set; }
        public AgencyPictureVDto Icon { get; set; }
    }

    /// <summary>ردیف تخت کوئری (EF نمی‌تواند Concat را روی پروجکشن تودرتو ترجمه کند)؛ بعد از صفحه‌بندی به AgencyVDto تبدیل می‌شود.</summary>
    public class AgencyRow
    {
        public int Kind { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public long? ProviderId { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public string ActivationValue { get; set; }
        public bool IsPersonal { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public DateTime? GoldAccountDate { get; set; }
        public DateTime? SilverAccountDate { get; set; }
        public long OwnerId { get; set; }
        public string OwnerFirstName { get; set; }
        public string OwnerLastName { get; set; }
        public string OwnerMobile { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string NeighborhoodName { get; set; }
        public string PictureUrl { get; set; }
        public string IconUrl { get; set; }

        public AgencyVDto ToView() => new AgencyVDto
        {
            Kind = Kind,
            Id = Id,
            Name = Name,
            Phone = Phone,
            ProviderId = ProviderId,
            Active = Active,
            Approved = Approved,
            ActivationValue = ActivationValue,
            IsPersonal = IsPersonal,
            RateAvg = RateAvg,
            RateCount = RateCount,
            GoldAccountDate = GoldAccountDate,
            SilverAccountDate = SilverAccountDate,
            OwnerId = OwnerId,
            Owner = OwnerId == 0 ? null : new AgencyOwnerVDto { Id = OwnerId, FirstName = OwnerFirstName, LastName = OwnerLastName, Mobile = OwnerMobile },
            City = CityName == null ? null : new AgencyCityVDto { Name = CityName, State = StateName == null ? null : new AgencyNameVDto { Name = StateName } },
            Neighborhood = NeighborhoodName == null ? null : new AgencyNameVDto { Name = NeighborhoodName },
            Picture = PictureUrl == null ? null : new AgencyPictureVDto { Url = PictureUrl },
            Icon = IconUrl == null ? null : new AgencyPictureVDto { Url = IconUrl },
        };
    }

    public class AgencySearchVDto
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<AgencyVDto> List { get; set; } = new List<AgencyVDto>();
    }
}
