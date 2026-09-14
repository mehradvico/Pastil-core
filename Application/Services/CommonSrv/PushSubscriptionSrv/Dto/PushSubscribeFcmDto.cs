using System;

namespace Application.Services.CommonSrv.PushSubscriptionSrv.Dto
{
    public class PushSubscribeFcmDto
    {
        public Guid DeviceKey { get; set; }
        public string FcmToken { get; set; }
        // "android" | "ios" | "windows"
        public string Platform { get; set; }
        public string UserAgent { get; set; }
    }
}
