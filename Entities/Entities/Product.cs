using Entities.Entities.CommonField;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Product : Seo_Full_Field, ISlugEntity
    {
        public string ProductLabel { get; set; }
        public string Slug { get; set; }
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
        public Point Location { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public long? VarietyId { get; set; }
        public long? Variety2Id { get; set; }
        public long? StoreId { get; set; }
        public string AdminDescription { get; set; }
        public DiscountGroup DiscountGroup { get; set; }
        public Variety Variety { get; set; }
        public Variety Variety2 { get; set; }
        public Brand Brand { get; set; }
        public Category Category { get; set; }
        public Code Status { get; set; }
        public Code Type { get; set; }
        public Picture Picture { get; set; }
        public Store Stores { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<ProductReport> ProductReports { get; set; }
        public ICollection<ProductFeatureValue> ProductFeatureValues { get; set; }
        public ICollection<ProductPicture> ProductPictures { get; set; }
        public ICollection<ProductFile> ProductFiles { get; set; }
        public ICollection<ProductComment> ProductComments { get; set; }
        public ICollection<ProductLike> ProductLikes { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<ProductItem> ProductItems { get; set; }

        public string GetSlugSource() => ProductLabel;
    }
}
