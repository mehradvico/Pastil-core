using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class PushPattern : Id_Field
    {
        public long PushTypeId { get; set; } 
        public string Title { get; set; }
        public string Body { get; set; }

        public string Url { get; set; }
        public string Icon { get; set; }
        public string Tag { get; set; }

        public bool IsActive { get; set; }
        public PushType PushType { get; set; }
    }


}
