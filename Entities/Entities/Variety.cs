using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Variety : Name_Field
    {
        public string Description { get; set; }
        public int Priority { get; set; }
        public bool InSearch { get; set; }
        public string Label { get; set; }
        public bool Deleted { get; set; }
        public ICollection<Product> Products { get; set; }
        public ICollection<VarietyItem> VarietyItems { get; set; }
    }
}
