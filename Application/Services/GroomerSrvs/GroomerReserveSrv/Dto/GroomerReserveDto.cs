using Application.Common.Dto.Input;
using System;
using System.Collections.Generic;

namespace Application.Services.GroomerSrvs.GroomerReserveSrv.Dto
{
    /// <summary>ورودی ساخت رزرو توسط کاربر. مبلغ‌ها از بدنه پذیرفته نمی‌شوند؛ همه از روی بسته‌ی قیمتی سرور محاسبه می‌شوند.</summary>
    public class GroomerReserveInputDto
    {
        public long GroomerId { get; set; }
        public long GroomerServicePackageId { get; set; }
        public long UserPetId { get; set; }
        /// <summary>بسته‌های اضافه (رزرو چندخدمتی)؛ بسته‌ی اصلی در GroomerServicePackageId است. حداکثر ۵ بسته، هر خدمت یک‌بار.</summary>
        public List<long> ExtraPackageIds { get; set; } = new List<long>();
        /// <summary>زمان شروع نوبت (باید روی یکی از زمان‌های آزاد آرایشگاه باشد).</summary>
        public DateTime DoDate { get; set; }
        public bool IsHomeService { get; set; }
        public long? AddressId { get; set; }
        public bool PickupRequested { get; set; }
        public long? AssigneeUserId { get; set; }
        public string BookerNote { get; set; }
    }

    public class GroomerReserveQuoteDto
    {
        public long GroomerServicePackageId { get; set; }
        public List<long> PackageIds { get; set; } = new List<long>();
        public double PackagePrice { get; set; }
        public double PickupFee { get; set; }
        public double Price { get; set; }
        /// <summary>مبلغی که همین حالا آنلاین پرداخت می‌شود (قبل از تخفیف).</summary>
        public double OnlineAmount { get; set; }
        /// <summary>باقی‌مانده که حضوری در آرایشگاه پرداخت می‌شود.</summary>
        public double PayAtVenue { get; set; }
        public int DurationMinutes { get; set; }
    }

    public class GroomerAvailabilitySlotDto
    {
        /// <summary>HH:mm</summary>
        public string Time { get; set; }
        public DateTime StartDate { get; set; }
        public int Capacity { get; set; }
        public int Available { get; set; }
    }

    public class GroomerAvailabilityDto
    {
        public DateTime Date { get; set; }
        public int DurationMinutes { get; set; }
        public List<GroomerAvailabilitySlotDto> Slots { get; set; } = new List<GroomerAvailabilitySlotDto>();
    }

    public class GroomerReserveItemVDto
    {
        public long Id { get; set; }
        public long GroomerServiceId { get; set; }
        public string ServiceName { get; set; }
        public long GroomerServicePackageId { get; set; }
        public string PackageName { get; set; }
        public double PackagePrice { get; set; }
        public double OnlineAmount { get; set; }
        public int DurationMinutes { get; set; }
        public decimal CommissionPercent { get; set; }
        public double ProviderShare { get; set; }
        public double SiteShare { get; set; }
    }

    public class GroomerReserveVDto
    {
        public int RescheduleCount { get; set; }
        public DateTime? OriginalDoDate { get; set; }
        public List<GroomerReserveItemVDto> Items { get; set; } = new List<GroomerReserveItemVDto>();
        public long Id { get; set; }
        public string ReserveCode { get; set; }
        public long GroomerId { get; set; }
        public string GroomerName { get; set; }
        public string GroomerPhone { get; set; }
        public string GroomerAddress { get; set; }
        public long GroomerServiceId { get; set; }
        public string ServiceName { get; set; }
        public long GroomerServicePackageId { get; set; }
        public string PackageName { get; set; }
        public string PackagePetSize { get; set; }
        public long BookerId { get; set; }
        public string BookerName { get; set; }
        public string BookerMobile { get; set; }
        public long UserPetId { get; set; }
        public string PetName { get; set; }
        public long? AssigneeUserId { get; set; }
        public string AssigneeName { get; set; }
        public DateTime DoDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsHomeService { get; set; }
        public long? AddressId { get; set; }
        public string AddressValue { get; set; }
        public bool PickupRequested { get; set; }
        public string BookerNote { get; set; }
        public double PackagePrice { get; set; }
        public double PickupFee { get; set; }
        public double Price { get; set; }
        public double OnlineAmount { get; set; }
        public double PayAtVenue { get; set; }
        public bool FromWallet { get; set; }
        public double WalletPrice { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        public double PaymentPrice { get; set; }
        public int Status { get; set; }
        public bool IsReserved { get; set; }
        public bool IsCancel { get; set; }
        public string CancelDetail { get; set; }
        public DateTime? CancelDate { get; set; }
        public bool CancelledByBooker { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? DoneDate { get; set; }
        /// <summary>مبلغ نهایی ثبت‌شده موقع تکمیل؛ برای رزرو تکمیل‌نشده/قدیمی = Price.</summary>
        public double FinalAmount { get; set; }
        /// <summary>مبلغ دریافتی در محل که ارائه‌دهنده ثبت کرد.</summary>
        public double ReceivedAtVenue { get; set; }
        /// <summary>true = باقی‌مانده دریافت نشد و بدهی مشتری شد (DebtAmount).</summary>
        public bool NotReceived { get; set; }
        public string CompleteDetail { get; set; }
        /// <summary>زمان تکمیل (همان DoneDate).</summary>
        public DateTime? CompletedAt { get; set; }
        public double DebtAmount { get; set; }
        /// <summary>زمان تسویه‌ی بدهی (کیف پول، دریافت مستقیم یا صفر کردن ادمین)؛ null = بدهی باز.</summary>
        public DateTime? DebtPaidDate { get; set; }
        public int OwnerDecision { get; set; }
        public DateTime? OwnerDecisionDate { get; set; }
        public string OwnerDecisionReason { get; set; }
        public DateTime? OwnerApprovalDeadline { get; set; }
        /// <summary>فقط برای ادمین/مالک: سهم‌ها بعد از پرداخت.</summary>
        public double ProviderShare { get; set; }
        public double SiteShare { get; set; }
        public decimal CommissionPercent { get; set; }
        public bool Permitted { get; set; }
        /// <summary>کاربر هنوز می‌تواند با لغو رایگان کل مبلغ را پس بگیرد.</summary>
        public bool FreeCancelAvailable { get; set; }
    }

    public class GroomerReserveSearchInputDto : BaseInputDto
    {
        public long? BookerId { get; set; }
        public long? GroomerId { get; set; }
        public long? ProviderId { get; set; }
        /// <summary>فقط رزروهای آرایشگاه‌های این مالک (اپ نماینده).</summary>
        public long? OwnerUserId { get; set; }
        /// <summary>فقط رزروهای سپرده‌شده به این پرسنل (پنل اپراتور؛ سرور پر می‌کند).</summary>
        public long? AssigneeUserId { get; set; }
        /// <summary>true = فقط رزروهایی که هنوز پرسنل ندارند.</summary>
        public bool? Unassigned { get; set; }
        public long? UserPetId { get; set; }
        public int? Status { get; set; }
        public int? OwnerDecision { get; set; }
        public bool? IsCancel { get; set; }
        /// <summary>true = فقط رزروهای پرداخت‌شده (پیش‌فرض ادمین/مالک)؛ false/خالی = همه.</summary>
        public bool? OnlyPaid { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class GroomerReservePage
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<GroomerReserveVDto> List { get; set; } = new List<GroomerReserveVDto>();
    }

    public class GroomerReserveRebateCodeDto
    {
        public long Id { get; set; }
        public string RebateCode { get; set; }
    }

    public class GroomerReserveWalletDto
    {
        public long Id { get; set; }
        public bool FromWallet { get; set; }
    }

    public class GroomerReserveRescheduleDto
    {
        public long Id { get; set; }
        public DateTime NewDoDate { get; set; }
    }

    public class GroomerReserveReasonDto
    {
        public long Id { get; set; }
        public string Reason { get; set; }
    }
}
