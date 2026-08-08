using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Feature : Name_Field, ISlugEntity
    {
        public string Label { get; set; }
        public string Slug { get; set; }
        public int Priority { get; set; }
        public bool Hide { get; set; }
        public bool InSearch { get; set; }
        public bool IsGroup { get; set; }
        public long TypeId { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<FeatureItem> FeatureItems { get; set; }
        public ICollection<ProductFeatureValue> ProductFeatureValues { get; set; }
        public Code Type { get; set; }

        public string GetSlugSource() => Label;
    }
}
