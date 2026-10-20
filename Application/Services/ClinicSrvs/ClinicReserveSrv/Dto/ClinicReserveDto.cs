using Application.Common.Dto.Input;
using System;
using System.Collections.Generic;

namespace Application.Services.ClinicSrvs.ClinicReserveSrv.Dto
{
    /// <summary>ورودی ساخت رزرو توسط کاربر. مبلغ‌ها از بدنه پذیرفته نمی‌شوند؛ همه از روی بسته‌ی قیمتی سرور محاسبه می‌شوند.</summary>
    public class ClinicReserveInputDto
    {
        public long ClinicId { get; set; }
        public long ClinicServicePackageId { get; set; }
        public long UserPetId { get; set; }
        /// <summary>بسته‌های اضافه (رزرو چندخدمتی)؛ بسته‌ی اصلی در ClinicServicePackageId است. حداکثر ۵ بسته، هر خدمت یک‌بار.</summary>
        public List<long> ExtraPackageIds { get; set; } = new List<long>();
        /// <summary>زمان شروع نوبت (باید روی یکی از زمان‌های آزاد کلینیک باشد).</summary>
        public DateTime DoDate { get; set; }
        /// <summary>نحوه‌ی ارائه: ۳۷ آنلاین، ۳۸ حضوری در کلینیک، ۳۹ در محل مشتری.</summary>
        public int Mode { get; set; }
        /// <summary>فقط برای Mode=۳۹: نشانی متعلق به مشتری و داخل محدوده‌ی کلینیک.</summary>
        public long? AddressId { get; set; }
        public long? AssigneeUserId { get; set; }
        public string BookerNote { get; set; }
    }

    public class ClinicReserveQuoteDto
    {
        public long ClinicServicePackageId { get; set; }
        public List<long> PackageIds { get; set; } = new List<long>();
        public int Mode { get; set; }
        public double PackagePrice { get; set; }
        public double Price { get; set; }
        /// <summary>مبلغی که همین حالا آنلاین پرداخت می‌شود (قبل از تخفیف).</summary>
        public double OnlineAmount { get; set; }
        /// <summary>باقی‌مانده که حضوری در کلینیک پرداخت می‌شود.</summary>
        public double PayAtVenue { get; set; }
        public int DurationMinutes { get; set; }
    }

    public class ClinicAvailabilitySlotDto
    {
        /// <summary>HH:mm</summary>
        public string Time { get; set; }
        public DateTime StartDate { get; set; }
        public int Capacity { get; set; }
        public int Available { get; set; }
    }

    public class ClinicAvailabilityDto
    {
        public DateTime Date { get; set; }
        public int DurationMinutes { get; set; }
        public List<ClinicAvailabilitySlotDto> Slots { get; set; } = new List<ClinicAvailabilitySlotDto>();
    }

    public class ClinicReserveItemVDto
    {
        public long Id { get; set; }
        public long ClinicServiceId { get; set; }
        public string ServiceName { get; set; }
        public long ClinicServicePackageId { get; set; }
        public string PackageName { get; set; }
        public double PackagePrice { get; set; }
        public double OnlineAmount { get; set; }
        public int DurationMinutes { get; set; }
        public decimal CommissionPercent { get; set; }
        public double ProviderShare { get; set; }
        public double SiteShare { get; set; }
        /// <summary>۱ در انتظار تأیید، ۲ تأییدشده، ۳ لغوشده.</summary>
        public int Status { get; set; }
        public string StatusReason { get; set; }
        public double RefundAmount { get; set; }
        public bool AddedAfterPayment { get; set; }
    }

    public class ClinicReserveVDto
    {
        public int RescheduleCount { get; set; }
        public DateTime? OriginalDoDate { get; set; }
        public DateTime? CallStartDate { get; set; }
        public DateTime? CallEndDate { get; set; }
        /// <summary>سفر پت‌رسانِ متصل (اگر بود)؛ پرداختش جدا از رزرو است.</summary>
        public Application.Services.TripSrv.TripSrv.Dto.LinkedPetResanTripVDto PetResanTrip { get; set; }
        public List<ClinicReserveItemVDto> Items { get; set; } = new List<ClinicReserveItemVDto>();
        public long Id { get; set; }
        public string ReserveCode { get; set; }
        public long ClinicId { get; set; }
        public string ClinicName { get; set; }
        public string ClinicPhone { get; set; }
        public string ClinicAddress { get; set; }
        public long ClinicServiceId { get; set; }
        public string ServiceName { get; set; }
        public long ClinicServicePackageId { get; set; }
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
        /// <summary>۳۷ آنلاین، ۳۸ حضوری در کلینیک، ۳۹ در محل مشتری.</summary>
        public int Mode { get; set; }
        public long? AddressId { get; set; }
        public string AddressValue { get; set; }
        public string BookerNote { get; set; }
        public double PackagePrice { get; set; }
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

    public class ClinicReserveSearchInputDto : BaseInputDto
    {
        public long? BookerId { get; set; }
        public long? ClinicId { get; set; }
        public long? ProviderId { get; set; }
        /// <summary>فقط رزروهای کلینیک‌های این مالک (اپ نماینده).</summary>
        public long? OwnerUserId { get; set; }
        public long? UserPetId { get; set; }
        public int? Status { get; set; }
        public int? OwnerDecision { get; set; }
        public bool? IsCancel { get; set; }
        /// <summary>true = فقط رزروهای پرداخت‌شده (پیش‌فرض ادمین/مالک)؛ false/خالی = همه.</summary>
        public bool? OnlyPaid { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class ClinicReservePage
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<ClinicReserveVDto> List { get; set; } = new List<ClinicReserveVDto>();
    }

    public class ClinicReserveRebateCodeDto
    {
        public long Id { get; set; }
        public string RebateCode { get; set; }
    }

    public class ClinicReserveWalletDto
    {
        public long Id { get; set; }
        public bool FromWallet { get; set; }
    }

    public class ClinicReserveRescheduleDto
    {
        public long Id { get; set; }
        public DateTime NewDoDate { get; set; }
    }

    public class ClinicReserveReasonDto
    {
        public long Id { get; set; }
        public string Reason { get; set; }
    }
}
