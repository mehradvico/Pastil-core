using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class PushNotification : Id_Field
    {
        public long UserId { get; set; }

        public long? NoticeId { get; set; }

        public long PushPatternId { get; set; }

        public bool IsSend { get; set; }

        public string Token1 { get; set; }
        public string Token2 { get; set; }
        public string Token3 { get; set; }
        public string Token4 { get; set; }
        public string Token5 { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public string Tag { get; set; }

        public bool? Status { get; set; }
        public string StatusText { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? SendDate { get; set; }
        public DateTime? SentDate { get; set; }
        public int AttemptCount { get; set; }
        public DateTime? NextAttemptDate { get; set; }

        public PushPattern PushPattern { get; set; }
        public Notice Notice { get; set; }
    }


}
