using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.ProductSrvs.ProductPictureSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.ProductSrvs.ProductSrv.Dto
{
    public class ProductDto : Seo_Full_FieldDto
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
        public int SellCount { get; set; }
        public int VisitCount { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }
        public long? StoreId { get; set; }
        public string AdminDescription { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public List<long> CategoryIds { get; set; }
        public PointDto Location { get; set; }
        public bool Active { get; set; }
        public long? VarietyId { get; set; }
        public long? Variety2Id { get; set; }
        public List<ProductPictureDto> ProductPictures { get; set; }
        public PictureVDto Picture { get; set; }
    }
}
