using Entities.Entities.CommonField;
using Entities.Entities.LocationField;
using NetTopologySuite.Geometries;
#pragma warning disable CS8632

namespace Entities.Entities
{
    public class Neighborhood : Name_Field
    {
        public int RegionNumber { get; set; }
        public long CityId { get; set; }
        public Geometry? Boundary { get; set; }

        public City City { get; set; }
    }
}
