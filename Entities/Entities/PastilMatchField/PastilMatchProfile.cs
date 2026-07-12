using Entities.Entities.CommonField;
using Entities.Entities.LocationField;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.PastilMatchField
{
    public class PastilMatchProfile : Id_Field
    {
        public long UserPetId { get; set; }
        public long EnergyLevelId { get; set; }
        public long SocialLevelId { get; set; }
        public int LikeCount { get; set; }
        public Point LiveLocation { get; set; }
        public long? CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public string Description { get; set; }

        public bool IsActive { get; set; }
        public bool? IsVerified { get; set; }
        public string AdminDescription { get; set; }
        public DateTime? VerificationDate { get; set; }

        public bool Deleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public DateTime LastActiveDate { get; set; }
        public DateTime CreateDate { get; set; }

        public UserPet UserPet { get; set; }
        public Code EnergyLevel { get; set; }
        public Code SocialLevel { get; set; }
        public City City { get; set; }
        public Neighborhood Neighborhood { get; set; }

        public ICollection<PastilMatchProfileGoal> PastilMatchProfileGoals { get; set; }
    }
}