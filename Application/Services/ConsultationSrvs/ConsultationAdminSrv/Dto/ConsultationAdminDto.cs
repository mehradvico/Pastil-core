using Application.Common.Dto.Input;
using Application.Services.Filing.PictureSrv.Dto;
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
        public long PackageId { get; set; }
        // نام پکیج در لحظه‌ی خرید
        public string PackageName { get; set; }
        public int ChannelId { get; set; }
        // مدتی که خریداری شده (دقیقه)
        public int DurationMinutes { get; set; }
        public double Price { get; set; }
        public double RebatePrice { get; set; }
        public double WalletPrice { get; set; }
        public double PaymentPrice { get; set; }
        public double CompanionShare { get; set; }
        public double SiteShare { get; set; }
        // مبلغی که واقعاً برای پاستیل ماند: پرداختیِ خریدهای پرداخت‌شده/فعال/تکمیل‌شده، وگرنه ۰ (لغو/منقضی/بازپرداخت‌شده)
        public double NetPaid { get; set; }
        // مبلغ برگشت‌داده‌شده به کیف پول کاربر (فقط خرید Refunded)
        public double RefundedAmount { get; set; }
        // true وقتی سهم کلینیک در یک تسویه آمده است
        public bool Permitted { get; set; }
        public int Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? StartDeadline { get; set; }
        // پنجره‌ی مشاوره: از «شروع» نماینده تا پایان
        public DateTime? StartDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        // تماس درون‌برنامه‌ای (صوتی/تصویری): مدتی که هر دو طرف واقعاً وصل بودند (ثانیه) و اولین/آخرین لحظه‌ی تماس.
        // برای چت و تماس تلفنی اندازه‌گیری نمی‌شود (۰)؛ برای چت تعداد پیام‌ها را ببینید.
        public int CallSeconds { get; set; }
        public DateTime? CallStartDate { get; set; }
        public DateTime? CallEndDate { get; set; }
        public int MessageCount { get; set; }
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

    // خلاصه‌ی گزارش مالی/مدت خریدهای مشاوره برای همان فیلترهای جستجو (بدون صفحه‌بندی)
    public class ConsultationPurchaseAdminSummaryDto
    {
        public int TotalCount { get; set; }
        // تعداد به تفکیک وضعیت (کلید = ConsultationPurchaseStatusEnum)
        public Dictionary<int, int> CountByStatus { get; set; } = new();
        // فروش ناخالص (قیمت پکیج) و تخفیف‌ها، فقط برای خریدهای پرداخت‌شده/فعال/تکمیل‌شده
        public double GrossPrice { get; set; }
        public double RebateTotal { get; set; }
        public double NetPaidTotal { get; set; }
        public double RefundedTotal { get; set; }
        // سهم‌ها (فقط خریدهای تکمیل‌شده که قابل تسویه‌اند) و وضعیت تسویه
        public double CompanionShareCompleted { get; set; }
        public double SiteShareCompleted { get; set; }
        public double SettledShare { get; set; }
        public double UnsettledShare { get; set; }
        // مدت خریداری‌شده‌ی خریدهای فعال/تکمیل‌شده (دقیقه) و مدت واقعی تماس‌های درون‌برنامه‌ای (ثانیه)
        public int PurchasedMinutes { get; set; }
        public long TalkSeconds { get; set; }
    }

    // پکیج‌های یک کلینیک (نمای ادمین)
    public class ConsultationPackageAdminVDto
    {
        public long Id { get; set; }
        public long CompanionId { get; set; }
        public string CompanionName { get; set; }
        public int ChannelId { get; set; }
        public int DurationMinutes { get; set; }
        public double Price { get; set; }
        public bool Active { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long? PictureId { get; set; }
        public PictureVDto Picture { get; set; }
        public int SortOrder { get; set; }
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
