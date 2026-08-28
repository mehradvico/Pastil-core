using Application.Common.Dto.Field;
using Application.Common.Enumerable.Code;
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
        public long? UserId { get; set; }

        /// <summary>
        /// تاریخ/ساعت ارسال یک‌باره (AutoSend=false)، یا لنگر زمان‌بندی برای ارسال خودکار (AutoSend=true)
        /// </summary>
        public DateTime? SendDate { get; set; }
        public bool AutoSend { get; set; }
        public PushRecurrenceEnum RecurrenceType { get; set; }
    }
}
