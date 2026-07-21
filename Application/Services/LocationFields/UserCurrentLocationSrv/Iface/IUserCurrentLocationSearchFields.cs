using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.UserCurrentLocationSrv.Iface
{
    public interface IUserCurrentLocationSearchFields
    {
        long? UserId { get; set; }
        long? CityId { get; set; }
        long? NeighborhoodId { get; set; }
    }
}
