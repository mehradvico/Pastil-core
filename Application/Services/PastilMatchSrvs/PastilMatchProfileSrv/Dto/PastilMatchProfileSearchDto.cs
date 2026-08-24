using Application.Common.Dto.LocationPoint;
using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Iface;
using AutoMapper;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileSrv.Dto
{
    public class PastilMatchProfileSearchDto : BaseSearchDto<PastilMatchProfile, PastilMatchProfileVDto>, IPastilMatchProfileSearchFields
    {
        public PastilMatchProfileSearchDto(PastilMatchProfileInputDto dto, IQueryable<PastilMatchProfile> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.UserPetId = dto.UserPetId;
            this.EnergyLevelId = dto.EnergyLevelId;
            this.SocialLevelId = dto.SocialLevelId;
            this.CityId = dto.CityId;
            this.NeighborhoodId = dto.NeighborhoodId;
            this.Username = dto.Username;
            this.IsVerified = dto.IsVerified;
            this.PastilMatchGoalIds = dto.PastilMatchGoalIds;
            this.LiveLocation = dto.LiveLocation;
            this.MaxDistanceInKilometers = dto.MaxDistanceInKilometers;
        }

        public long? UserPetId { get; set; }
        public long? EnergyLevelId { get; set; }
        public long? SocialLevelId { get; set; }
        public long? CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public string Username { get; set; }
        public bool? IsVerified { get; set; }
        public List<long> PastilMatchGoalIds { get; set; }
        public PointDto LiveLocation { get; set; }
        public double? MaxDistanceInKilometers { get; set; }
    }
}
