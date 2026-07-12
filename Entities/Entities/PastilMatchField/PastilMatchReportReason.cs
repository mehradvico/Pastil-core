using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.PastilMatchField
{
    public class PastilMatchReportReason : Id_Field
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public int Priority { get; set; }
        public bool IsDescriptionRequired { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
    }
}
