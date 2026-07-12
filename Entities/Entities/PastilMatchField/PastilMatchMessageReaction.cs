using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.PastilMatchField
{
    public class PastilMatchMessageReaction : Id_Field
    {
        public long PastilMatchMessageId { get; set; }
        public long ReactorProfileId { get; set; }

        public string Reaction { get; set; }
        public bool Deleted { get; set; }

        public PastilMatchMessage PastilMatchMessage { get; set; }
        public PastilMatchProfile ReactorProfile { get; set; }
    }
}
