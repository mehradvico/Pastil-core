using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushBroadcastSrv.Dto
{
    public class PushPayloadDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public string Tag { get; set; }
    }

}
