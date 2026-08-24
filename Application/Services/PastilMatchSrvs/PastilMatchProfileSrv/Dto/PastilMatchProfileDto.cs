using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto
{
    public class PastilMatchProfileDto : Id_FieldDto
    {
        public long UserPetId { get; set; }
        public long EnergyLevelId { get; set; }
        public long SocialLevelId { get; set; }
        public int LikeCount { get; set; }
        public PointDto LiveLocation { get; set; }
        public long? CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public string Username { get; set; }
        public string Description { get; set; }

        public bool IsActive { get; set; }
        public bool? IsVerified { get; set; }
        public string AdminDescription { get; set; }
        public DateTime? VerificationDate { get; set; }

        public bool Deleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public DateTime LastActiveDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
