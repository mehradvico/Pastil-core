using System;
using System.Collections.Generic;

namespace Application.Services.ClinicSrvs.ClinicReserveItemSrv.Dto
{
    /// <summary>یک بسته داخل رزرو با وضعیتش. Status: 1 در انتظار تأیید، 2 تأییدشده، 3 لغوشده.</summary>
    public class ClinicReserveItemVDto
    {
        public long Id { get; set; }
        public long PackageId { get; set; }
        public long ServiceId { get; set; }
        public string PackageName { get; set; }
        public double Price { get; set; }
        /// <summary>سهم آنلاین این بسته (۰ برای بسته‌ی اضافه‌شده بعد از پرداخت).</summary>
        public double OnlineAmount { get; set; }
        public int Status { get; set; }
        public string StatusReason { get; set; }
        public DateTime? StatusChangedDate { get; set; }
        public bool StatusChangedByAdmin { get; set; }
        /// <summary>مبلغی که با لغو این بسته به کیف پول کاربر برگشته.</summary>
        public double RefundAmount { get; set; }
        public DateTime? RefundDate { get; set; }
        public bool AddedAfterPayment { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class ClinicReserveItemSummaryVDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Cancelled { get; set; }
        public double RefundedTotal { get; set; }
    }

    public class ClinicReserveItemsVDto
    {
        public long ReserveId { get; set; }
        /// <summary>true = کلینیک/ادمین هنوز می‌تواند تأیید/لغو/افزودن کند (رزرو پرداخت‌شده و لغو/تکمیل‌نشده).</summary>
        public bool CanManage { get; set; }
        public List<ClinicReserveItemVDto> Items { get; set; } = new List<ClinicReserveItemVDto>();
        public ClinicReserveItemSummaryVDto Summary { get; set; } = new ClinicReserveItemSummaryVDto();
    }

    public class ClinicReserveItemStatusDto
    {
        /// <summary>2 = تأیید، 3 = لغو</summary>
        public int Status { get; set; }
        /// <summary>برای لغو الزامی (به کاربر نشان داده می‌شود).</summary>
        public string Reason { get; set; }
    }

    public class ClinicReserveItemAddDto
    {
        public long PackageId { get; set; }
    }

    public class ClinicReserveItemStatusResultVDto
    {
        public ClinicReserveItemVDto Item { get; set; }
        public double RefundAmount { get; set; }
        /// <summary>true = آخرین بسته‌ی فعال هم لغو شد و کل رزرو لغو شد.</summary>
        public bool ReserveCancelled { get; set; }
        public ClinicReserveItemsVDto Items { get; set; }
    }
}
