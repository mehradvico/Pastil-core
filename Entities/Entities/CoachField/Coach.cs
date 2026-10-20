using Entities.Entities.LocationField;
using Entities.Entities.Security;
using Entities.Entities.CommonField;
using Entities.Entities.ProviderField;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace Entities.Entities.CoachField
{
    /// <summary>
    /// مربی حیوانات (Coach): نمایندگی مستقل زیر یک ارائه‌دهنده؛ جدا شده از Companion.
    /// خدمات، پرسنل، ساعت کاری، رزرو و پرداخت هر کدام جدول خودشان را دارند (Coach*).
    /// </summary>
    public class Coach : Seo_Full_Field
    {
        public long? ProviderId { get; set; }
        /// <summary>کاربر مالک (همان کاربر ارائه‌دهنده).</summary>
        public long OwnerId { get; set; }
        /// <summary>اگر از انتقال داده‌ی Companion ساخته شده: شناسه‌ی ردیف قدیمی (برای ردیابی و مخفی‌کردن از لیست نمایندگی‌ها).</summary>
        public long? LegacyCompanionId { get; set; }
        public string ReferralCode { get; set; }

        public long? PictureId { get; set; }
        public long? BackgroundPictureId { get; set; }
        public long? IconId { get; set; }

        public string Phone { get; set; }
        public string AddressValue { get; set; }
        public long? StateId { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public Point Location { get; set; }

        public bool Active { get; set; }
        public bool ShowToSite { get; set; }
        public bool Approved { get; set; }
        public string ActivationValue { get; set; }
        public bool Suggested { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public long? DeletedByUserId { get; set; }
        public DateTime CreateDate { get; set; }

        public int CommentCount { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public string SearchKey { get; set; }

        // اشتراک (از ProviderSubscriptionSync همگام می‌شود؛ الماس مثل طلایی)
        public DateTime? GoldAccountDate { get; set; }
        public DateTime? SilverAccountDate { get; set; }

        // ---- فیلدهای جدید مربی ----
        public string Regulations { get; set; }
        public int? EstablishedYear { get; set; }
        public string LicenseNumber { get; set; }
        public bool LicenseApproved { get; set; }
        public string InstagramUrl { get; set; }
        public string WebsiteUrl { get; set; }
        /// <summary>خدمات در محل مشتری (با پوشش CoachZone).</summary>
        public bool ServiceAtHome { get; set; }
        /// <summary>خدمات حضوری در خود مربی.</summary>
        public bool ServiceInPerson { get; set; }
        /// <summary>خدمات آنلاین (مشاوره‌ی تصویری/چت).</summary>
        public bool ServiceOnline { get; set; }
        public bool HasParking { get; set; }
        public bool HasWaitingRoom { get; set; }
        public bool HasCamera { get; set; }
        /// <summary>ظرفیت هم‌زمان حیوان در مربی؛ ۰ = نامحدود/تعریف‌نشده.</summary>
        public int MaxConcurrentPets { get; set; }
        public int StaffCount { get; set; }
        /// <summary>درصد کمیسیون پیش‌فرض سایت از خدمات این مربی (خدمت می‌تواند مقدار خودش را داشته باشد).</summary>
        public decimal CommissionPercent { get; set; }

        public Provider Provider { get; set; }
        public User Owner { get; set; }
        public Picture Picture { get; set; }
        public Picture BackgroundPicture { get; set; }
        public Picture Icon { get; set; }
        public State State { get; set; }
        public City City { get; set; }
        public Neighborhood Neighborhood { get; set; }

        public ICollection<CoachUser> CoachUsers { get; set; }
        public ICollection<CoachService> CoachServices { get; set; }
        public ICollection<CoachTime> CoachTimes { get; set; }
        public ICollection<CoachZone> CoachZones { get; set; }
        public ICollection<CoachPet> CoachPets { get; set; }
        public ICollection<CoachPicture> CoachPictures { get; set; }
        public ICollection<CoachComment> CoachComments { get; set; }
    }
}
