using System;
using System.Collections.Generic;

namespace Application.Services.ProviderSrvs.ProviderOverviewSrv.Dto
{
    /// <summary>خلاصه‌ی کامل یک ارائه‌دهنده برای صفحه‌ی جزئیات ادمین: حسابداری، کیف پول، کارت‌های بانکی، تسویه‌ها و نمایندگی‌ها.</summary>
    public class ProviderOverviewVDto
    {
        public double WalletAmount { get; set; }
        public ProviderFinanceSummaryVDto Finance { get; set; } = new ProviderFinanceSummaryVDto();
        public List<ProviderVenueOverviewVDto> Venues { get; set; } = new List<ProviderVenueOverviewVDto>();
        public List<ProviderBankCardVDto> BankCards { get; set; } = new List<ProviderBankCardVDto>();
        public List<ProviderSettlementVDto> RecentSettlements { get; set; } = new List<ProviderSettlementVDto>();
    }

    /// <summary>جمع حسابداری هر دو استخر ارائه‌دهنده (نمایندگی‌ها + فروشگاه‌ها). «تسویه‌شده» = ردیف‌های Permitted؛ «تسویه‌نشده» = مانده‌ی قابل پرداخت.</summary>
    public class ProviderFinanceSummaryVDto
    {
        public int ItemCount { get; set; }
        public double ProviderShare { get; set; }
        public double SiteShare { get; set; }
        public double SettledShare { get; set; }
        public double UnsettledShare { get; set; }
        /// <summary>جمع مبالغ پرداخت‌شده در تسویه‌ها (جدول Settlements).</summary>
        public double PaidOut { get; set; }
        public int SettlementCount { get; set; }
        public DateTime? LastSettlementDate { get; set; }
    }

    public class ProviderVenueOverviewVDto
    {
        /// <summary>Companion یا Store</summary>
        public string Kind { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public double RateAvg { get; set; }
        public int RateCount { get; set; }

        // حسابداری این نمایندگی
        public int ItemCount { get; set; }
        public double ProviderShare { get; set; }
        public double SiteShare { get; set; }
        public double SettledShare { get; set; }
        public double UnsettledShare { get; set; }
        // فقط Store: سفارش‌هایی که مشتری «تحویل نگرفتم» زده (قبل از تسویه باید بررسی شوند)
        public int NotReceivedCount { get; set; }

        // فقط Companion
        public int ServiceCount { get; set; }
        public int StaffCount { get; set; }
        public int PansionCount { get; set; }
        public int SchoolCount { get; set; }
    }

    public class ProviderBankCardVDto
    {
        public long Id { get; set; }
        public string BankName { get; set; }
        public string CardNumber { get; set; }
        public string ShebaNumber { get; set; }
        public string CardHolderName { get; set; }
        public bool Approved { get; set; }
    }

    public class ProviderSettlementVDto
    {
        public long Id { get; set; }
        /// <summary>Companion یا Store: کدام استخر تسویه شده.</summary>
        public string Kind { get; set; }
        public double PaidPrice { get; set; }
        public long ItemCount { get; set; }
        public string TrackingCode { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
