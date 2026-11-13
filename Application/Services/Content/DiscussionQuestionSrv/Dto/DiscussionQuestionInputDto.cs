using Application.Common.Dto.Input;
using Application.Services.Content.DiscussionQuestionSrv.Iface;
using System;

namespace Application.Services.Content.DiscussionQuestionSrv.Dto
{
    public class DiscussionQuestionInputDto : BaseInputDto, IDiscussionQuestionSearchFields
    {
        public bool? Active { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? AdminConfirm { get; set; }
        public long? ProductId { get; set; }
        /// <summary>فقط ادمین: پرسش‌هایی که حداقل یک پاسخ در انتظار تأیید دارند.</summary>
        public bool? HasPendingAnswers { get; set; }
        /// <summary>فقط ادمین: پرسش‌هایی که هنوز بررسی نشده‌اند (AdminConfirm = null).</summary>
        public bool? Pending { get; set; }

    }
}
