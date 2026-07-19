using Entities.Entities.CommonField;
using Entities.Entities.LocationField;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class State : Name_Field
    {
        public string EnName { get; set; }
        public long CountryId { get; set; }
            public MultiPolygon Boundary { get; set; }
        public Country Country { get; set; }
        public ICollection<City> Cities { get; set; }
    }
}
