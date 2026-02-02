using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class PushMessage : Id_Field
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Url { get; set; }
        public long PictureId { get; set; }
        public string Tag { get; set; }

        public long PushMessageTypeId { get; set; }
        public bool Deleted { get; set; }

        public Picture Picture { get; set; }
        public Code PushMessageType { get; set; }
    }
}
