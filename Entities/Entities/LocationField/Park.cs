using Entities.Entities.CommonField;
using Entities.Entities.PansionField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.LocationField
{
    public class Park : Name_Field
    {
        public long NeighborhoodId { get; set; }
        public bool Suggested { get; set; }
        public long? PictureId { get; set; } 

        public Neighborhood Neighborhood { get; set; }
        public Picture Picture { get; set; }
        public ICollection<ParkPicture> ParkPictures { get; set; }
    }
}
