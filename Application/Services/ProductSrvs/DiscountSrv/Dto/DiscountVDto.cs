using Application.Common.Dto.Field;
using Application.Services.CategorySrv.Dto;
using Application.Services.ProductSrvs.BrandSrv.Dto;
using Application.Services.ProductSrvs.DiscountGroupSrv.Dto;
using Application.Services.ProductSrvs.ProductItemSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using System;

namespace Application.Services.ProductSrvs.DiscountSrv.Dto
{
    public class DiscountVDto : Id_FieldDto
    {
        public long TypeId { get; set; }
        public long DiscountGroupId { get; set; }
        public DateTime? EndDate { get; set; }
        public long StoreId { get; set; }
        public int Percent { get; set; }
        public bool Synced { get; set; }
        public long? CategoryId { get; set; }
        public long? BrandId { get; set; }
        public long? ProductId { get; set; }
        public long? ProductItemId { get; set; }
        public bool Active { get; set; }
        public bool FundedByPastil { get; set; }

        public DiscountGroupVDto DiscountGroup { get; set; }
        public CodeVDto Type { get; set; }
        public CategoryVDto Category { get; set; }
        public BrandVDto Brand { get; set; }
        public ProductVDto Product { get; set; }
        public ProductItemVDto ProductItem { get; set; }
        public StoreVDto Store { get; set; }
    }
}
