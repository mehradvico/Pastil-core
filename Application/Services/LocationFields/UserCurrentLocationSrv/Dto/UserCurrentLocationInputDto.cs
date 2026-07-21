using Application.Common.Dto.Input;
using Application.Services.LocationFields.UserCurrentLocationSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.UserCurrentLocationSrv.Dto
{
    public class UserCurrentLocationInputDto : BaseInputDto, IUserCurrentLocationSearchFields
    {
        public long? UserId { get; set; }
        public long? CityId { get; set; }
        public long? NeighborhoodId { get; set; }
    }
}
