using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Question : FullName_Field
    {
        public int Priority { get; set; }
        public bool Active { get; set; }
    }
}
