using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class CodeGroup : Name_Field
    {
        public string Label { get; set; }
        public ICollection<Code> Codes { get; set; }
    }
}
