using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushSubscriptionSrv.Dto
{
    public class PushSubscribeDto
    {
        public Guid DeviceKey { get; set; }
        public string Endpoint { get; set; }
        public PushKeysDto Keys { get; set; }
        public string UserAgent { get; set; }
    }
}
