using Application.Common.Dto.Field;
using Application.Common.Dto.LocationPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.ParkSrv.Dto
{
    public class ParkDto : Name_FieldDto
    {
        public long NeighborhoodId { get; set; }
        public bool Suggested { get; set; }
        public long? PictureId { get; set; }
        public string AddressValue { get; set; }
        public PointDto Location { get; set; }
    }
}
