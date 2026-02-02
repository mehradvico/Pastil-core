using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushBroadcastSrv.Dto
{
    namespace Application.Services.CommonSrv.PushBroadcastSrv.Dto
    {
        public class PushPayloadDto
        {
            public string Title { get; set; }
            public string Body { get; set; }
            public string Url { get; set; }
            public string Icon { get; set; }
            public string Tag { get; set; }
        }
    }
}
