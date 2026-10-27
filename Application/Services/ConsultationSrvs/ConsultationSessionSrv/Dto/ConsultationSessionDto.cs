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
        // مالک خرید: دقیقاً یکی پر است؛ CompanionName نام همان مرکز است
        public long? CompanionId { get; set; }
        public long? ClinicId { get; set; }
        public long? CoachId { get; set; }
        public string CompanionName { get; set; }
        public string PackageName { get; set; }
        public int ChannelId { get; set; }
        public int DurationMinutes { get; set; }
        public int Status { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? StartDeadline { get; set; }
        // رزرو ساعت‌دار: ساعت رزرو کاربر (خرید فوری: null). نماینده از ۱۰ دقیقه قبل از آن می‌تواند «شروع» را بزند.
        public DateTime? ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }
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
        // پت(های) این مشاوره. از ۲۰۲۶-۱۰-۲۷ کاربر هنگام خرید پت(ها) را انتخاب می‌کند و فقط همان‌ها می‌آیند
        // (HasSelectedPets = true)؛ برای خریدهای قدیمی‌تر همه‌ی پت‌های مشتری (HasSelectedPets = false).
        // پروفایل کامل و سابقه‌ی پزشکی: GET /api/Companion/ConsultationSession/{id}/Pets
        public List<ConsultationCustomerPetVDto> CustomerPets { get; set; }
        public bool HasSelectedPets { get; set; }
        public DateTime ServerNow { get; set; }
    }

    public class ConsultationCustomerPetVDto
    {
        public long UserPetId { get; set; }
        public string Name { get; set; }
        public string PetName { get; set; }
        public PictureVDto Picture { get; set; }
    }

    // پروفایل کامل پت برای نماینده‌ی کلینیک/مربی در یک مشاوره‌ی آنلاین
    public class ConsultationPetProfileVDto
    {
        public long UserPetId { get; set; }
        public string Name { get; set; }
        // نوع حیوان (سگ، گربه، …)
        public string PetName { get; set; }
        public string BreedName { get; set; }
        public string Breed2Name { get; set; }
        public bool IsMixBreed { get; set; }
        public bool IsMale { get; set; }
        public bool IsSterile { get; set; }
        public DateTime Birthday { get; set; }
        // سن به ماه؛ متن «۲ سال و ۳ ماه» را کلاینت می‌سازد
        public int AgeMonths { get; set; }
        public string Size { get; set; }
        public string Weight { get; set; }
        public string MicroChipCode { get; set; }
        // بیماری/داروی خاصی که خود صاحب پت ثبت کرده
        public string SpecificDisease { get; set; }
        public string SpecificMedicine { get; set; }
        public PictureVDto Picture { get; set; }
        // سوابق ثبت‌شده توسط کلینیک‌ها/اپراتورها (UserPetRecord)، جدیدترین اول
        public List<ConsultationPetRecordVDto> Records { get; set; } = new();
        // مراجعات تکمیل‌شده‌ی کلینیک (حضوری/در محل/آنلاین)، جدیدترین اول
        public List<ConsultationPetVisitVDto> ClinicVisits { get; set; } = new();
        // مشاوره‌های آنلاین قبلی همین پت (به‌جز همین خرید)، جدیدترین اول
        public List<ConsultationPetVisitVDto> PastConsultations { get; set; } = new();
    }

    public class ConsultationPetRecordVDto
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public string OperatorName { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class ConsultationPetVisitVDto
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string VenueName { get; set; }
        public string Title { get; set; }
        // یادداشت تکمیل (فقط مراجعه‌ی کلینیک)
        public string Note { get; set; }
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
