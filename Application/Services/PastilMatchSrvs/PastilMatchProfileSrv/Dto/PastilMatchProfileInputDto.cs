using Application.Common.Dto.Input;
using Application.Common.Dto.LocationPoint;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto
{
    public class PastilMatchProfileInputDto : BaseInputDto, IPastilMatchProfileSearchFields
    {
        public long? UserPetId { get; set; }
        public long? EnergyLevelId { get; set; }
        public long? SocialLevelId { get; set; }
        public long? CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public bool? IsVerified { get; set; }
        public List<long> PastilMatchGoalIds { get; set; }
        public PointDto LiveLocation { get; set; }
        public double? MaxDistanceInKilometers { get; set; }
    }
}
