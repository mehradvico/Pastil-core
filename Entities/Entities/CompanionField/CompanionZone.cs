using Entities.Entities.CommonField;
using Entities.Entities.LocationField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.CompanionField
{
    public class CompanionZone : Id_Field
    {
        public long CompanionId { get; set; }
        public long StateId { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public bool Deleted { get; set; }

        public Companion Companion { get; set; }
        public State State { get; set; }
        public City City { get; set; }
        public Neighborhood Neighborhood { get; set; }
    }
}
