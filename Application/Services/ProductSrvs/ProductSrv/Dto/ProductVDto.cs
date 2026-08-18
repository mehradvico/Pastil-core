using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Services.CategorySrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.ProductSrvs.BrandSrv.Dto;
using Application.Services.ProductSrvs.DiscountGroupSrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using System;

namespace Application.Services.ProductSrvs.ProductSrv.Dto
{
    public class ProductVDto : Seo_FieldDto
    {
        public string Slug { get; set; }
        public string ProductLabel { get; set; }

        public string SecondName { get; set; }
        public int? SellLimitCount { get; set; }

        public long? CategoryId { get; set; }
        public Nullable<long> BrandId { get; set; }
        public Nullable<long> PictureId { get; set; }
        public long StatusId { get; set; }
        public long TypeId { get; set; }
        public string CodeValue { get; set; }
        public long BasePrice { get; set; }
        public long Price { get; set; }
        public int DiscountPercent { get; set; }
        public long? DiscountGroupId { get; set; }
        public DateTime? DiscountEndDate { get; set; }

        public int SellCount { get; set; }
        public int VisitCount { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public long? VarietyId { get; set; }
        public long? Variety2Id { get; set; }
        public long? StoreId { get; set; }
        public string AdminDescription { get; set; }
        public int? ShippingWeightGrams { get; set; }
        public decimal? ShippingLengthCm { get; set; }
        public decimal? ShippingWidthCm { get; set; }
        public decimal? ShippingHeightCm { get; set; }
        public bool Active { get; set; }
        public StoreMinVDto Store { get; set; }
        public PointDto Location { get; set; }
        public CategoryVDto Category { get; set; }
        public CodeDto Status { get; set; }
        public CodeDto Type { get; set; }
        public PictureVDto Picture { get; set; }
        public BrandVDto Brand { get; set; }
        public DiscountGroupVDto DiscountGroup { get; set; }
    }
}
