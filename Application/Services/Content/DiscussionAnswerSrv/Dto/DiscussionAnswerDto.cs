using Application.Common.Dto.Field;

namespace Application.Services.Content.DiscussionAnswerSrv.Dto
{
    public class DiscussionAnswerDto : Id_FieldDto
    {
        public string Content { get; set; }
        public long DiscussionQuestionId { get; set; }
        public long UserId { get; set; }
        public bool Active { get; set; }
        /// <summary>پاسخ به یک پاسخ مشخص (اختیاری)؛ خالی = پاسخ مستقیم به پرسش.</summary>
        public long? ReplyToAnswerId { get; set; }
    }
}
