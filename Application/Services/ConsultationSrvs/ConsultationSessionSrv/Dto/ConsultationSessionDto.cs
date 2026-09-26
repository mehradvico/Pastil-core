using Application.Services.Filing.PictureSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.ConsultationSrvs.ConsultationSessionSrv.Dto
{
    // یک خرید در صف/جریان برای نماینده («مشاوره‌های من»)
    public class ConsultationAgentItemVDto
    {
        public long Id { get; set; }
        public string PurchaseCode { get; set; }
        public long CompanionId { get; set; }
        public string CompanionName { get; set; }
        public string PackageName { get; set; }
        public int ChannelId { get; set; }
        public int DurationMinutes { get; set; }
        public int Status { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? StartDeadline { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public long? OnlineSessionId { get; set; }
        // مشتری
        public long UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }
        public PictureVDto UserPicture { get; set; }
        // نماینده‌ای که شروع کرده (نال = هنوز شروع نشده)
        public long? AgentUserId { get; set; }
        // این کاربر همین الان می‌تواند «شروع» یا «ورود» را بزند
        public bool CanStart { get; set; }
        public bool CanEnter { get; set; }
        // فقط مالک: می‌تواند مشاوره‌ی شروع‌نشده را به یک نماینده تخصیص دهد
        public bool CanAssign { get; set; }
        // نام نماینده‌ی تخصیص‌یافته (تا قبل از شروع) یا شروع‌کننده
        public string AgentName { get; set; }
        // نمایندگان قابل تخصیص (فقط وقتی CanAssign = true)
        public List<ConsultationAssignableAgentVDto> AssignableAgents { get; set; }
        public DateTime ServerNow { get; set; }
    }

    public class ConsultationAssignableAgentVDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
    }

    // نتیجه‌ی شروع/ورود: نماینده بر اساس کانال به صفحه‌ی مناسب هدایت می‌شود
    public class ConsultationSessionInfoVDto
    {
        public long PurchaseId { get; set; }
        public long OnlineSessionId { get; set; }
        public int ChannelId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public DateTime ServerNow { get; set; }
        public long UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }
    }

    // پنجره‌ی فعال مشاوره برای کاربر (نوار «بازگشت به مشاوره»)
    public class ConsultationActiveWindowVDto
    {
        public long PurchaseId { get; set; }
        public long OnlineSessionId { get; set; }
        public string PackageName { get; set; }
        public int ChannelId { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public DateTime ServerNow { get; set; }
        public string CompanionName { get; set; }
        public string AgentFullName { get; set; }
    }
}
