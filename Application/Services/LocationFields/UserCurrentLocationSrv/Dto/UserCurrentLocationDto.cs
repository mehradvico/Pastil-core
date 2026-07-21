using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.LocationFields.UserCurrentLocationSrv.Dto
{
    public class UserCurrentLocationDto : Id_FieldDto
    {
        public long UserId { get; set; }

        [Required]
        public PointDto Location { get; set; }

        [Range(1, long.MaxValue)]
        public long CityId { get; set; }
        public long? NeighborhoodId { get; set; }

        public DateTime LastUpdateDate { get; set; }
    }
}
