using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class ProductFeatureValue : Name_Field
    {
        public long ProductId { get; set; }
        public long? FeatureItemId { get; set; }
        public long FeatureId { get; set; }
        public Product Product { get; set; }
        public Feature Feature { get; set; }
        public FeatureItem FeatureItem { get; set; }
    }
}
