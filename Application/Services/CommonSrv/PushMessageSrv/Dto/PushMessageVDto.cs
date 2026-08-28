using Application.Common.Dto.Field;
using Application.Common.Enumerable.Code;
using Application.Services.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.PushBroadcastSrv.Dto
{
    public class PushMessageVDto : Id_FieldDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Url { get; set; }
        public long PictureId { get; set; }
        public string Tag { get; set; }

        public long PushMessageTypeId { get; set; }
        public long? UserId { get; set; }
        public bool Deleted { get; set; }

        public DateTime? SendDate { get; set; }
        public bool AutoSend { get; set; }
        public PushRecurrenceEnum RecurrenceType { get; set; }
        public DateTime? LastSentDate { get; set; }

        public PictureVDto Picture { get; set; }
        public CodeVDto PushMessageType { get; set; }
        public UserMinVDto User { get; set; }
    }
}
