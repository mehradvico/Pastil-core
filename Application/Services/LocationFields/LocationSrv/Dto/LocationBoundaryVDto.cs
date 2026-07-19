using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.LocationSrv.Dto
{
    public class LocationBoundaryVDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public MultiPolygon Boundary { get; set; }
    }
}
