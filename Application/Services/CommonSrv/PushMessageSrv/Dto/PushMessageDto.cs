using Application.Common.Dto.Field;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushBroadcastSrv.Dto
{
    public class PushMessageDto : Id_FieldDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Url { get; set; }
        public long PictureId { get; set; }
        public string Tag { get; set; }

        public long PushMessageTypeId { get; set; }
    }
}
