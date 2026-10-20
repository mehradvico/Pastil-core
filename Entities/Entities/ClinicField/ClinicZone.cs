using Entities.Entities.LocationField;
using Entities.Entities.CommonField;

namespace Entities.Entities.ClinicField
{
    /// <summary>محدوده‌ی پوشش خدمات در محل (معادل CompanionZone).</summary>
    public class ClinicZone : Id_Field
    {
        public long ClinicId { get; set; }
        public long StateId { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public bool Deleted { get; set; }

        public Clinic Clinic { get; set; }
        public State State { get; set; }
        public City City { get; set; }
        public Neighborhood Neighborhood { get; set; }
    }
}
