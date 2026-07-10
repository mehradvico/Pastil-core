using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Code : Name_Field
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public long CodeGroupId { get; set; }
        public int Priority { get; set; }
        public bool Active { get; set; }
        public CodeGroup CodeGroup { get; set; }
        public ICollection<CompanionAssistance> CompanionAssistances { get; set; }
        public ICollection<Companion> Companions { get; set; }
    }
}
