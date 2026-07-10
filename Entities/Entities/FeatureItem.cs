using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class FeatureItem : Name_Field
    {
        public long FeatureId { get; set; }
        public int Priority { get; set; }
        public bool Deleted { get; set; }
        public Feature Feature { get; set; }
        public ICollection<ProductFeatureValue> ProductFeatureValues { get; set; }
    }
}
