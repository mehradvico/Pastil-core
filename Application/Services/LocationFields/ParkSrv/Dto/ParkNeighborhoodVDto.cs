using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.ParkSrv.Dto
{
    public class ParkNeighborhoodVDto : Name_FieldDto
    {
        public long NeighborhoodId { get; set; }
        public string NeighborhoodName { get; set; }
        public string FullName { get; set; }
    }
}
