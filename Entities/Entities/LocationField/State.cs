using Entities.Entities.CommonField;
using Entities.Entities.LocationField;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

#pragma warning disable CS8632

namespace Entities.Entities
{
    public class State : Name_Field
    {
        public string EnName { get; set; }
        public long CountryId { get; set; }
        public Geometry? Boundary { get; set; }
        public Country Country { get; set; }
        public ICollection<City> Cities { get; set; }
    }
}
