using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.LocationField
{
    public class ParkPicture : Id_Field
    {
        public long ParkId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
        public bool Deleted { get; set; }

        public Park Park { get; set; }
        public Picture Picture { get; set; }
    }
}
