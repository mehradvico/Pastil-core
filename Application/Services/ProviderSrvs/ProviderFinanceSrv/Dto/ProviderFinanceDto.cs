using Application.Common.Dto.Input;
using Entities.Entities;
using System;
using System.Collections.Generic;

namespace Application.Services.ProviderSrvs.ProviderFinanceSrv.Dto
{
    public class ProviderFinanceInputDto : BaseInputDto
    {
        /// <summary>true = فقط ارائه‌دهندگانی که مانده‌ی تسویه‌نشده دارند.</summary>
        public bool? HasUnsettled { get; set; }
    }

    /// <summary>جمع حسابداری یک استخر (نمایندگی‌ها یا فروشگاه‌ها) برای یک ارائه‌دهنده.</summary>
    public class ProviderPoolSummaryVDto
    {
        public SettlementKindEnum Kind { get; set; }
        public int ItemCount { get; set; }
        public int UnsettledCount { get; set; }
        public double ProviderShare { get; set; }
        public double SiteShare { get; set; }
        public double SettledShare { get; set; }
        public double UnsettledShare { get; set; }
        /// <summary>جمع مبلغ تسویه‌های ثبت‌شده‌ی این استخر.</summary>
        public double PaidOut { get; set; }
        public int SettlementCount { get; set; }
        public DateTime? LastSettlementDate { get; set; }
        /// <summary>فقط فروشگاه: سفارش‌های تسویه‌نشده که مشتری «تحویل نگرفتم» زده.</summary>
        public int NotReceivedCount { get; set; }
        /// <summary>فقط فروشگاه: سفارش‌هایی که فروشگاه‌های چند ارائه‌دهنده در آن‌اند؛ سهم سفارش یک‌جا به اولین تسویه می‌رسد و باید دستی بررسی شود.</summary>
        public int SharedOrderCount { get; set; }
    }

    public class ProviderFinanceVenueVDto
    {
        public SettlementKindEnum Kind { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public int ItemCount { get; set; }
        public int UnsettledCount { get; set; }
        public double ProviderShare { get; set; }
        public double SiteShare { get; set; }
        public double SettledShare { get; set; }
        public double UnsettledShare { get; set; }
    }

    public class ProviderFinanceRowVDto
    {
        public long ProviderId { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public bool Active { get; set; }
        public int CompanionCount { get; set; }
        public int StoreCount { get; set; }
        public int GroomerCount { get; set; }
        public int ClinicCount { get; set; }
        public int CoachCount { get; set; }
        public int SchoolCount { get; set; }
        public double CompanionUnsettled { get; set; }
        public double StoreUnsettled { get; set; }
        public double GroomerUnsettled { get; set; }
        public double ClinicUnsettled { get; set; }
        public double CoachUnsettled { get; set; }
        public double SchoolUnsettled { get; set; }
        public double TotalUnsettled { get; set; }
        public double TotalSettled { get; set; }
        public double PaidOut { get; set; }
        public DateTime? LastSettlementDate { get; set; }
        public bool HasApprovedCard { get; set; }
    }

    public class ProviderUnassignedVenueVDto
    {
        public SettlementKindEnum Kind { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>نتیجه‌ی لیست حسابداری؛ صفحه‌بندی در حافظه است چون مانده‌ها محاسبه‌شده‌اند.</summary>
    public class ProviderFinanceSearchVDto
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<ProviderFinanceRowVDto> List { get; set; } = new List<ProviderFinanceRowVDto>();
        /// <summary>نمایندگی/فروشگاهِ حذف‌نشده‌ای که ارائه‌دهنده ندارد؛ تا وصل نشوند تسویه نمی‌شوند.</summary>
        public List<ProviderUnassignedVenueVDto> UnassignedVenues { get; set; } = new List<ProviderUnassignedVenueVDto>();
    }

    public class ProviderFinanceCardVDto
    {
        public long Id { get; set; }
        public string BankName { get; set; }
        public string CardNumber { get; set; }
        public string ShebaNumber { get; set; }
        public string CardHolderName { get; set; }
        public bool Approved { get; set; }
    }

    public class ProviderFinanceDetailVDto
    {
        public long ProviderId { get; set; }
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public bool Active { get; set; }
        public ProviderPoolSummaryVDto CompanionPool { get; set; }
        public ProviderPoolSummaryVDto StorePool { get; set; }
        public ProviderPoolSummaryVDto GroomerPool { get; set; }
        public ProviderPoolSummaryVDto ClinicPool { get; set; }
        public ProviderPoolSummaryVDto CoachPool { get; set; }
        // ثبت‌نام مدرسه‌های ارائه‌دهنده (SettlementKindEnum.School)
        public ProviderPoolSummaryVDto SchoolPool { get; set; }
        public List<ProviderFinanceVenueVDto> Venues { get; set; } = new List<ProviderFinanceVenueVDto>();
        public List<ProviderFinanceCardVDto> Cards { get; set; } = new List<ProviderFinanceCardVDto>();
    }

    public class ProviderPendingItemVDto
    {
        public long VenueId { get; set; }
        public string VenueName { get; set; }
        /// <summary>Reserve | Pansion | Consultation | School | Order | Groomer | Clinic | Coach</summary>
        public string Type { get; set; }
        public string Code { get; set; }
        public string BookerFullName { get; set; }
        public double PaymentPrice { get; set; }
        public decimal CommissionPercent { get; set; }
        public double SiteShare { get; set; }
        public double ProviderShare { get; set; }
        public string StatusLabel { get; set; }
        /// <summary>فقط سفارش فروشگاه: مشتری «تحویل نگرفتم» زده.</summary>
        public bool NotReceived { get; set; }
    }

    public class ProviderPendingBreakdownVDto
    {
        public string Type { get; set; }
        public int Count { get; set; }
        public double PaymentPrice { get; set; }
        public double SiteShare { get; set; }
        public double ProviderShare { get; set; }
    }

    /// <summary>پیش‌نمایش دقیق آنچه با «ثبت تسویه» پرداخت می‌شود.</summary>
    public class ProviderPendingVDto
    {
        public SettlementKindEnum Kind { get; set; }
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
        public int ItemCount { get; set; }
        public double PaymentPrice { get; set; }
        public double SiteShare { get; set; }
        public double ProviderShare { get; set; }
        public int NotReceivedCount { get; set; }
        public List<ProviderPendingBreakdownVDto> Breakdown { get; set; } = new List<ProviderPendingBreakdownVDto>();
        public List<ProviderPendingItemVDto> Items { get; set; } = new List<ProviderPendingItemVDto>();
        public List<ProviderFinanceCardVDto> Cards { get; set; } = new List<ProviderFinanceCardVDto>();
    }
}
