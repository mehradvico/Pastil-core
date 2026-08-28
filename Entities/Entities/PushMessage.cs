using Entities.Entities.CommonField;
using Entities.Entities.Security;
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
        public long? UserId { get; set; }
        public bool Deleted { get; set; }

        /// <summary>
        /// تاریخ/ساعت ارسال یک‌باره (AutoSend=false)، یا لنگر (زمان/روز هفته/روز ماه/روز سال) برای ارسال خودکار تکرارشونده (AutoSend=true)
        /// </summary>
        public DateTime? SendDate { get; set; }
        /// <summary>
        /// وقتی true است، پیام بدون دخالت ادمین طبق RecurrenceType به‌صورت خودکار و تکرارشونده ارسال می‌شود
        /// </summary>
        public bool AutoSend { get; set; }
        /// <summary>
        /// مقدار (Application.Common.Enumerable.Code.PushRecurrenceEnum): None=0, Daily=1, Weekly=2, Monthly=3, Yearly=4
        /// </summary>
        public int RecurrenceType { get; set; }
        /// <summary>
        /// آخرین باری که این پیام (یک‌باره یا یک دوره‌ی تکرار) واقعاً ارسال شد؛ برای جلوگیری از ارسال دوباره در همان دوره
        /// </summary>
        public DateTime? LastSentDate { get; set; }

        public Picture Picture { get; set; }
        public Code PushMessageType { get; set; }
        public User User { get; set; }
    }
}
