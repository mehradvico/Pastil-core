using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Pet : Name_Field, ISlugEntity
    {
        public string Label { get; set; }
        public string Slug { get; set; }
        public bool Deleted { get; set; }
        public bool Active { get; set; }
        public int Priority { get; set; }
        public ICollection<Companion> Companions { get; set; }

        public string GetSlugSource() => Label;
    }
}
