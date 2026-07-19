using Entities.Entities.CommonField;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace Entities.Entities.LocationField
{
    public class City : Name_Field
    {
        public long StateId { get; set; }
        public Geometry? Boundary { get; set; }
        public State State { get; set; }   
    }
}
