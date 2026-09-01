using Entities.Entities.CommonField;
using Entities.Entities.Security;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class TripAddress : Name_Field
    {
        public Point Address { get; set; }
        public long UserId { get; set; }
        public bool Deleted { get; set; }
        public bool IsSelected { get; set; }

        public User User { get; set; }
    }
}
