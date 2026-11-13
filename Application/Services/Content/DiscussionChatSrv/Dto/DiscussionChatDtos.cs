using System;
using System.Collections.Generic;

namespace Application.Services.Content.DiscussionChatSrv.Dto
{
    public enum DiscussionChatKind
    {
        Question = 1,
        Answer = 2,
    }

    public class DiscussionChatEditDto
    {
        public DiscussionChatKind Kind { get; set; }
        public long Id { get; set; }
        public string Content { get; set; }
    }

    /// <summary>یکی از پیام‌های خودِ کاربر که هنوز تأیید نشده (Status=0) یا اخیراً رد شده (Status=2).</summary>
    public class DiscussionMyItemVDto
    {
        public DiscussionChatKind Kind { get; set; }
        public long Id { get; set; }
        public string Content { get; set; }
        public DateTime CreateDate { get; set; }
        /// <summary>0 = در انتظار تأیید ادمین، 2 = رد شده.</summary>
        public int Status { get; set; }
        public long DiscussionQuestionId { get; set; }
        public long? ReplyToAnswerId { get; set; }
    }

    public class DiscussionMyStatusVDto
    {
        /// <summary>true = کاربر (در همه‌ی کالاها) پیام تأییدنشده دارد و فعلاً نمی‌تواند پیام جدید بفرستد.</summary>
        public bool HasPending { get; set; }
        /// <summary>پیام‌های در انتظار/ردشده‌ی اخیرِ کاربر برای همین کالا، جدیدترین اول.</summary>
        public List<DiscussionMyItemVDto> Items { get; set; } = new List<DiscussionMyItemVDto>();
    }
}
