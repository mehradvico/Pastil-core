using Application.Common.Dto.Field;
using Application.Services.Content.DiscussionAnswerSrv.Dto;
using Application.Services.Dto;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.Content.DiscussionQuestionSrv.Dto
{
    public class DiscussionQuestionVDto : Id_FieldDto
    {
        public string Content { get; set; }
        public DateTime CreateDate { get; set; }
        public long ProductId { get; set; }
        public long UserId { get; set; }
        public bool? AdminConfirm { get; set; }
        public bool Active { get; set; }
        public int AnswerCount { get; set; }
        /// <summary>تعداد پاسخ‌های در انتظار تأیید این پرسش (فقط برای ادمین پر می‌شود).</summary>
        public int PendingAnswerCount { get; set; }

        public ProductVDto Product { get; set; }
        public UserMinVDto User { get; set; }
        public List<DiscussionAnswerVDto> DiscussionAnswers { get; set; }
    }
}
