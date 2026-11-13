using Application.Common.Dto.Field;
using Application.Services.Dto;
using System;

namespace Application.Services.Content.DiscussionAnswerSrv.Dto
{
    public class DiscussionAnswerVDto : Id_FieldDto
    {
        public string Content { get; set; }
        public long DiscussionQuestionId { get; set; }
        public long UserId { get; set; }
        public DateTime CreateDate { get; set; }
        public bool Active { get; set; }
        public bool? AdminConfirm { get; set; }
        public long? ReplyToAnswerId { get; set; }
        /// <summary>فروشگاهِ پاسخ‌دهنده (اگر «پاسخ فروشگاه» باشد).</summary>
        public long? StoreId { get; set; }
        public string StoreName { get; set; }
        public bool IsStoreAnswer { get; set; }
        public int LikeCount { get; set; }
        public int DisLikeCount { get; set; }
        public bool? UserIsLike { get; set; }

        public UserVDto User { get; set; }
    }
}
