using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Services.CategorySrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.ProductSrvs.BrandSrv.Dto;
using Application.Services.ProductSrvs.DiscountGroupSrv.Dto;
using Application.Services.ProductSrvs.ProductFeatureValueSrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.ProductSrv.Dto
{
    public class ProductExcelVDto : Seo_FieldDto
    {
        public string ProductLabel { get; set; }
        public string SecondName { get; set; }
        public int? SellLimitCount { get; set; }

        public long? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public long? BrandId { get; set; }
        public string BrandName { get; set; }
        public long? PictureId { get; set; }
        public long StatusId { get; set; }
        public string StatusName { get; set; }
        public long TypeId { get; set; }
        public string TypeName { get; set; }
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
        public string VarietyName { get; set; }
        public long? Variety2Id { get; set; }
        public string Variety2Name { get; set; }
        public long? StoreId { get; set; }
        public string AdminDescription { get; set; }
        public bool Active { get; set; }
        public StoreMinVDto Store { get; set; }
        public PointDto Location { get; set; }
        public CategoryVDto Category { get; set; }
        public CodeDto Status { get; set; }
        public CodeDto Type { get; set; }
        public PictureVDto Picture { get; set; }
        public BrandVDto Brand { get; set; }
        public DiscountGroupVDto DiscountGroup { get; set; }

        public List<ProductFeatureValueVDto> ProductFeatureValues { get; set; }
    }
}
