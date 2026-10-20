using Entities.Entities.LocationField;
using Entities.Entities.CommonField;

namespace Entities.Entities.GroomerField
{
    /// <summary>محدوده‌ی پوشش خدمات در محل (معادل CompanionZone).</summary>
    public class GroomerZone : Id_Field
    {
        public long GroomerId { get; set; }
        public long StateId { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public bool Deleted { get; set; }

        public Groomer Groomer { get; set; }
        public State State { get; set; }
        public City City { get; set; }
        public Neighborhood Neighborhood { get; set; }
    }
}
