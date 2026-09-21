using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities
{
    public class OnlineSessionMessage : Id_Field
    {
        public long OnlineSessionId { get; set; }
        public long SenderUserId { get; set; }
        public string Content { get; set; }
        // تصویر پیام (آدرس فایل روی سرور فایل)؛ متن پیام در این حالت کپشن اختیاری است.
        public string ImageUrl { get; set; }
        public string ImageThumbnailUrl { get; set; }

        public DateTime? DeliveredDate { get; set; }
        public DateTime? ReadDate { get; set; }

        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }

        public OnlineSession OnlineSession { get; set; }
        public User SenderUser { get; set; }
    }
}
