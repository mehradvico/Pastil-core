using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Category : Seo_Full_Field, ISlugEntity
    {
        public long? ParentId { get; set; }
        public string Label { get; set; }
        public string Slug { get; set; }
        public int Priority { get; set; }
        public long? PictureId { get; set; }
        public long? IconId { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public ICollection<Category> Children { get; set; }
        public Category Parent { get; set; }
        public ICollection<Product> Products { get; set; }
        public ICollection<Feature> Features { get; set; }
        public Picture Picture { get; set; }
        public Picture Icon { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<Brand> Brands { get; set; }
        public ICollection<Variety> Varieties { get; set; }
        public ICollection<Banner> Banner { get; set; }

        public string GetSlugSource() => Label;
    }
}
