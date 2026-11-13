using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class DiscussionAnswer : Id_Field
    {
        public string Content { get; set; }
        public long DiscussionQuestionId { get; set; }
        public long UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public int LikeCount { get; set; }
        public int DisLikeCount { get; set; }
        public bool Deleted { get; set; }
        public bool Active { get; set; }
        /// <summary>null = در انتظار تأیید ادمین، true = تأیید و نمایش، false = ردشده. فقط true (و Active) برای عموم نمایش داده می‌شود.</summary>
        public bool? AdminConfirm { get; set; }
        /// <summary>پاسخ به پیام (پاسخ) مشخصی از همین گفت‌وگو؛ null = پاسخ مستقیم به خود پرسش. (بدون FK: همان پرسش را سرویس بررسی می‌کند.)</summary>
        public long? ReplyToAnswerId { get; set; }
        /// <summary>اگر نویسنده از پرسنل فروشگاهی باشد که این کالا را می‌فروشد = آن فروشگاه («پاسخ فروشگاه»).</summary>
        public long? StoreId { get; set; }


        public DiscussionQuestion DiscussionQuestion { get; set; }
        public Store Store { get; set; }
        public User User { get; set; }
        public ICollection<DiscussionAnswerLike> DiscussionAnswerLikes { get; set; }

    }
}
