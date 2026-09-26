using System;
using System.Collections.Generic;

namespace Application.Services.CompanionSrvs.CompanionReserveUnifiedSrv.Dto
{
    public static class UnifiedReserveKinds
    {
        public const string Service = "service";
        public const string Consultation = "consultation";
    }

    public class UnifiedReserveInputDto
    {
        // all | service | consultation
        public string Type { get; set; } = "all";
        public long? CompanionId { get; set; }
        public long? BookerId { get; set; }
        // کد رزرو/خرید، نام یا موبایل رزروکننده
        public string Q { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UnifiedReserveVDto
    {
        // service = رزرو خدمت نماینده (CompanionReserve)، consultation = خرید بسته‌ی مشاوره آنلاین (ConsultationPurchase)
        public string Kind { get; set; }
        public long Id { get; set; }
        public string Code { get; set; }
        public string BookerName { get; set; }
        public string BookerMobile { get; set; }
        public long CompanionId { get; set; }
        public string CompanionName { get; set; }
        // نام خدمت (رزرو خدمت) یا نام پکیج (مشاوره)
        public string Title { get; set; }
        // فقط مشاوره: ۱ چت، ۲ تماس درون‌برنامه، ۳ تصویری، ۴ تلفنی (ConsultationChannel)
        public int? ChannelId { get; set; }
        public int? DurationMinutes { get; set; }
        // زمان رزرو (رزرو خدمت) یا شروع مشاوره / زمان خرید (مشاوره)
        public DateTime? DoDate { get; set; }
        public double PaymentPrice { get; set; }
        public bool IsCancel { get; set; }
        // رزرو خدمت: نام وضعیت رزرو؛ مشاوره: خالی
        public string StatusName { get; set; }
        // مشاوره: ConsultationPurchaseStatusEnum؛ رزرو خدمت: خالی
        public int? ConsultationStatus { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class UnifiedReserveListDto
    {
        public List<UnifiedReserveVDto> List { get; set; } = new();
        public int TotalCount { get; set; }
        public int ServiceCount { get; set; }
        public int ConsultationCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
