using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Services.Accounting.UserPetSrv.Dto;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.NeighborhoodSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchProfileGoalSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto
{
    public class PastilMatchProfileVDto : Id_FieldDto
    {
        public long UserPetId { get; set; }
        public long EnergyLevelId { get; set; }
        public long SocialLevelId { get; set; }
        public int LikeCount { get; set; }
        public PointDto LiveLocation { get; set; }
        public long? CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public string Description { get; set; }

        public bool IsActive { get; set; }
        public bool? IsVerified { get; set; }
        public string AdminDescription { get; set; }
        public DateTime? VerificationDate { get; set; }

        public DateTime LastActiveDate { get; set; }
        public DateTime CreateDate { get; set; }

        public UserPetVDto UserPet { get; set; }
        public CityVDto City { get; set; }
        public NeighborhoodVDto Neighborhood { get; set; }

        public List<PastilMatchProfileGoalVDto> PastilMatchProfileGoals { get; set; }
    }
}
