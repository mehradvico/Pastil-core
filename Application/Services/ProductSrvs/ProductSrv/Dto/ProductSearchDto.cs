using Application.Common.Dto.LocationPoint;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.ProductSrv.Iface;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.ProductSrv.Dto
{
    public class ProductSearchDto : BaseSearchDto<Product, ProductVDto>, IProductSearchFields
    {
        public ProductSearchDto(ProductInputDto dto, IQueryable<Product> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.Active = dto.Active;
            this.Available = dto.Available;
            this.NotId = dto.NotId;
            this.CategoryIds = dto.CategoryIds;
            this.CategoryLabels = dto.CategoryLabels;
            this.IsAndCategories = dto.IsAndCategories;
            this.PriceFrom = dto.PriceFrom;
            this.PriceTo = dto.PriceTo;
            this.InStock = dto.InStock;
            this.Status = dto.Status;
            this.Type = dto.Type;
            this.Distance = dto.Distance;
            this.FeatureItemIds = dto.FeatureItemIds;
            this.StoreId = dto.StoreId;
            this.HasDiscount = dto.HasDiscount;
            this.DiscountGroupLabel = dto.DiscountGroupLabel;
            this.IsAdmin = dto.IsAdmin;
            this.CreateStoreId = dto.CreateStoreId;

        }

        public long[] FeatureItemIds { get; set; }

        /// <summary>
        /// عبارتی که عملاً جستجو شد، فقط وقتی با Q فرق دارد (غلط املایی اصلاح شد، کیبورد اشتباه بود یا کلمه‌ای کنار گذاشته شد)؛
        /// وگرنه null. برای نمایش «نتایج برای …» در کلاینت.
        /// </summary>
        public string CorrectedQ { get; set; }

        /// <summary>کلمه‌هایی از Q که هیچ محصولی نداشتند و برای خالی‌نماندن نتیجه کنار گذاشته شدند؛ وگرنه null.</summary>
        public string[] IgnoredWords { get; set; }

        public bool? Active { get; set; }
        public long? NotId { get; set; }
        public long[] BrandIds { get; set; }
        public long[] CategoryIds { get; set; }
        public string[] CategoryLabels { get; set; }
        public bool IsAndCategories { get; set; }
        public double? PriceFrom { get; set; }
        public double? PriceTo { get; set; }
        public bool? InStock { get; set; }
        public long? StoreId { get; set; }
        public long? CreateStoreId { get; set; }

        public string DiscountGroupLabel { get; set; }
        public bool? HasDiscount { get; set; }
        public bool IsAdmin { get; set; }

        public ProductStatusEnum? Status { get; set; }
        public ProductTypeEnum? Type { get; set; }
        public PointDto Distance { get; set; }
    }
}
