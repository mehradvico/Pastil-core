using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Country : Name_Field
    {
        public string EnName { get; set; }
        public ICollection<State> States { get; set; }
    }
}
