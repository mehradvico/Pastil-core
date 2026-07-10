using Application.Common.Dto.Field;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.NeighborhoodSrv.Dto;
using Application.Services.LocationFields.StateSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionZoneSrv.Dto
{
    public class CompanionZoneVDto : Id_FieldDto
    {
        public long CompanionId { get; set; }
        public long StateId { get; set; }
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }

        public StateVDto State { get; set; }
        public CityVDto City { get; set; }
        public NeighborhoodVDto Neighborhood { get; set; }
    }
}
