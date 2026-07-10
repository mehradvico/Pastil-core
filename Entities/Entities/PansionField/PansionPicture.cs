using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.PansionField
{
    public class PansionPicture : Id_Field
    {
        public long PansionId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
        public bool Deleted { get; set; }

        public Pansion Pansion { get; set; }
        public Picture Picture { get; set; }
    }
}
