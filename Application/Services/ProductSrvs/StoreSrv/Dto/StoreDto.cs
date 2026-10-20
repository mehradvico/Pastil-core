using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Services.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using System.Collections.Generic;

namespace Application.Services.ProductSrvs.StoreSrv.Dto
{
    public class StoreDto : Seo_Full_FieldDto
    {
        public string ReferralCode { get; set; }
        /// <summary>ارائه‌دهنده‌ی صاحب فروشگاه. هنگام ساخت توسط ادمین الزامی است و فقط همان‌جا خوانده می‌شود؛ ویرایش بعدی از «اتصال به ارائه‌دهنده» انجام می‌شود (فروشنده با ویرایش فروشگاه نمی‌تواند آن را عوض کند).</summary>
        public long? ProviderId { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public PointDto Location { get; set; }
        public long? PictureId { get; set; }
        public long TypeId { get; set; }
        public long CityId { get; set; }
        public long? IconId { get; set; }
        public int MaxDiscountPercent { get; set; }

        /// <summary>حداکثر زمان آماده‌سازی سفارش (دقیقه، ۰ تا ۱۰۰۸۰). ندادن مقدار یعنی بدون تغییر.</summary>
        public int? MaxPreparationMinutes { get; set; }
        /// <summary>فعال‌سازی ارسال اکسپرس؛ null = بدون تغییر (کلاینت‌های قدیمی مقدار را خاموش نمی‌کنند).</summary>
        public bool? ExpressShippingEnabled { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public string ApprovalValue { get; set; }
        public bool ShowToSite { get; set; }
        public PictureVDto Picture { get; set; }
        public List<UserVDto> Users { get; set; }

    }
}
