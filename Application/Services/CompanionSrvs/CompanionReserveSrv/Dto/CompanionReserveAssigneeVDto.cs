namespace Application.Services.CompanionSrvs.CompanionReserveSrv.Dto
{
    public class CompanionReserveAssigneeVDto
    {
        // null وقتی همکار هنوز به این خدمت متصل نیست (فقط با includeAll=true دیده می‌شود)
        public long? CompanionAssistanceUserId { get; set; }
        // شناسه‌ی عضویت در کلینیک؛ برای تخصیص همکارِ «غیرمتصل» (اتصال خودکار) به‌جای CompanionAssistanceUserId ارسال می‌شود
        public long CompanionUserId { get; set; }
        public bool IsLinkedToService { get; set; }
        public long UserId { get; set; }
        public string FullName { get; set; }
        public long? PictureId { get; set; }
        public bool IsFemale { get; set; }
        public long? ExpertiseId { get; set; }
        public string ExpertiseName { get; set; }
        // همه‌ی تخصص‌های همکار به ترتیب انتخاب (تخصص اصلی اول)
        public System.Collections.Generic.List<string> Expertises { get; set; } = new();
        public bool IsAssigned { get; set; }
        // true وقتی یکی از تخصص‌های همکار با تخصص‌های مرتبطِ همین خدمت (AssistanceExpertise) یکی است
        public bool IsRecommended { get; set; }
    }
}
