using Application.Common.Dto.Input;
using System;
using System.Collections.Generic;

namespace Application.Services.ConsultationSrvs.ConsultationAdminSrv.Dto
{
    public class ConsultationPurchaseAdminInputDto : BaseInputDto
    {
        public long? CompanionId { get; set; }
        public long? UserId { get; set; }
        public int? Status { get; set; }
        public int? ChannelId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class ConsultationPurchaseAdminVDto
    {
        public long Id { get; set; }
        public string PurchaseCode { get; set; }
        public long CompanionId { get; set; }
        public string CompanionName { get; set; }
        public long UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }
        public int ChannelId { get; set; }
        public int DurationMinutes { get; set; }
        public double Price { get; set; }
        public double RebatePrice { get; set; }
        public double WalletPrice { get; set; }
        public double PaymentPrice { get; set; }
        public double CompanionShare { get; set; }
        public double SiteShare { get; set; }
        public int Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? StartDeadline { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public string CancelReason { get; set; }
        public DateTime? RefundDate { get; set; }
        public long? AgentUserId { get; set; }
        public string AgentFullName { get; set; }
        public long? OnlineSessionId { get; set; }
        public long? PaymentId { get; set; }
    }

    public class ConsultationPurchaseAdminSearchDto
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<ConsultationPurchaseAdminVDto> List { get; set; } = new();
    }

    // بسته‌های یک کلینیک (فقط خواندن)
    public class ConsultationPackageAdminVDto
    {
        public long Id { get; set; }
        public long CompanionId { get; set; }
        public string CompanionName { get; set; }
        public int ChannelId { get; set; }
        public int DurationMinutes { get; set; }
        public double Price { get; set; }
        public bool Active { get; set; }
    }

    // کلینیک‌هایی که حداقل یک بسته‌ی فعال دارند (نمای خلاصه برای ادمین)
    public class ConsultationClinicAdminVDto
    {
        public long CompanionId { get; set; }
        public string CompanionName { get; set; }
        public int ActivePackages { get; set; }
        public double MinPrice { get; set; }
        public double MaxPrice { get; set; }
        public int PurchasesCount { get; set; }
    }
}
