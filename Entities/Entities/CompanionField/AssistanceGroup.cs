using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class AssistanceGroup : Name_Field
    {
        public int Priority { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public ICollection<Assistance> Assistances { get; set; }
    }
}
