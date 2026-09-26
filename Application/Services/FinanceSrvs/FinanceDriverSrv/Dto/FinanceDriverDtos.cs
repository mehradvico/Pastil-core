using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.FinanceSrvs.FinanceDriverSrv.Dto
{
    public class FinanceDriverInputDto
    {
        public string Q { get; set; }
        public bool? Available { get; set; }
        // true = کمیسیون تنظیم‌شده (> ۰)، false = بدون کمیسیون
        public bool? HasCommission { get; set; }
        // ۱ = جدیدترین، ۲ = قدیمی‌ترین
        public int SortBy { get; set; } = 1;
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class FinanceDriverVDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string OwnerMobile { get; set; }
        public string Vehicle { get; set; }
        public string LicensePlateNumber { get; set; }
        public bool Active { get; set; }
        public bool Approved { get; set; }
        public decimal? CommissionPercent { get; set; }
        public int TripCount { get; set; }
        public int CompletedTripCount { get; set; }
        public int CanceledTripCount { get; set; }
        public double TotalPayment { get; set; }
        public double TotalDriverShare { get; set; }
        public double TotalSiteShare { get; set; }
    }

    public class FinanceDriverListDto
    {
        public List<FinanceDriverVDto> List { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class FinanceDriverTripVDto
    {
        public long TripId { get; set; }
        public DateTime CreateDate { get; set; }
        public long TripStatusId { get; set; }
        public double Price { get; set; }
        public double PaymentPrice { get; set; }
        public double DriverShare { get; set; }
        public double SiteShare { get; set; }
        public bool IsPaid { get; set; }
    }

    public class FinanceDriverDetailVDto : FinanceDriverVDto
    {
        public List<FinanceDriverTripVDto> Trips { get; set; } = new();
    }

    public class FinanceDriverCommissionDto
    {
        public long Id { get; set; }
        [Range(0, 100)]
        public decimal CommissionPercent { get; set; }
    }
}
