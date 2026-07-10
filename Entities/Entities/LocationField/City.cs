using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities.LocationField
{
    public class City : Name_Field
    {
        public long StateId { get; set; }
        public State State { get; set; }    }
}
