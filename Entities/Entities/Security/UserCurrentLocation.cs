using Entities.Entities.CommonField;
using Entities.Entities.LocationField;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Entities.Entities.Security
{
    public class UserCurrentLocation : Id_Field
    {
        public long UserId { get; set; }
        [Required]
        public Point Location { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }

        public DateTime LastUpdateDate { get; set; }

        public User User { get; set; }
        public City City { get; set; }
        public Neighborhood Neighborhood { get; set; }
    }
}
