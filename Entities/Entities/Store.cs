using Entities.Entities.CommonField;
using Entities.Entities.LocationField;
using Entities.Entities.ProviderField;
using Entities.Entities.Security;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Store : Seo_Full_Field
    {
        public string ReferralCode { get; set; }
        /// <summary>ارائه‌دهنده‌ی صاحب فروشگاه. Users (جدول StoreUser) پرسنل فروشگاه هستند.</summary>
        public long? ProviderId { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public Point Location { get; set; }
        public long TypeId { get; set; }
        public long CityId { get; set; }
        public long? PictureId { get; set; }
        public long? IconId { get; set; }
        public DateTime CreateDate { get; set; }
        public int MaxDiscountPercent { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public string ApprovalValue { get; set; }
        public bool ShowToSite { get; set; }
        public bool Deleted { get; set; }
        public int CommentCount { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public decimal CommissionPercent { get; set; }
        /// <summary>حداکثر زمان آماده‌سازی سفارش این فروشگاه (دقیقه). بازه‌های تحویل میاره بر اساس همین زمان برای مشتری نمایش داده می‌شوند.</summary>
        public int MaxPreparationMinutes { get; set; } = 120;
        /// <summary>فروشگاه «ارسال اکسپرس» (همان‌روز، ۲ تا ۳ ساعت کاری با پیک میاره + Boost) را فعال کرده است.</summary>
        public bool ExpressShippingEnabled { get; set; }
        public Picture Picture { get; set; }
        public Picture Icon { get; set; }
        public Code Type { get; set; }
        public City City { get; set; }
        public Provider Provider { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<StoreComment> StoreComments { get; set; }
    }
}
