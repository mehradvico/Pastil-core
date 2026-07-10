using Application.Common.Dto.Input;
using Application.Services.LocationFields.ParkSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.ParkSrv.Dto
{
    public class ParkInputDto : BaseInputDto, IParkSearchFields
    {
        public long? NeighborhoodId { get; set; }
        public long? CityId { get; set; }
        public long? StateId { get; set; }
    }
}
