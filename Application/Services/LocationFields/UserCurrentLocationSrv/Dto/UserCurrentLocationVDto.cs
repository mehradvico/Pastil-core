using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.NeighborhoodSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.UserCurrentLocationSrv.Dto
{
    public class UserCurrentLocationVDto : Id_FieldDto
    {
        public long UserId { get; set; }
        public PointDto Location { get; set; }

        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }
        public DateTime LastUpdateDate { get; set; }

        public CityVDto City { get; set; }
        public NeighborhoodVDto Neighborhood { get; set; }
    }
}
