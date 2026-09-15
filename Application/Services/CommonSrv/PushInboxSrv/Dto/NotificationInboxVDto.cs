using Application.Common.Dto.Field;
using System;

namespace Application.Services.CommonSrv.PushInboxSrv.Dto
{
    public class NotificationInboxVDto : Id_FieldDto
    {
        public string Title { get; set; }
        public string Body { get; set; }

        /// <summary>مسیر داخلی برای ناوبری (همان مقداری که در payload پوش هم می‌رود).</summary>
        public string Url { get; set; }

        public string Icon { get; set; }
        public string Tag { get; set; }

        /// <summary>دسته‌ی اعلان از روی PushType.Label — معادل data.type در payload پوش.</summary>
        public string Type { get; set; }

        public DateTime CreateDate { get; set; }

        /// <summary>زمان پذیرش توسط Provider؛ null یعنی تحویل Push موفق نبوده (اعلان همچنان معتبر است).</summary>
        public DateTime? SentDate { get; set; }

        public bool IsRead { get; set; }
        public DateTime? ReadDateUtc { get; set; }
    }
}
