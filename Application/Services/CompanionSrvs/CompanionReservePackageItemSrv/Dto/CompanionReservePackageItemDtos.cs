using System;
using System.Collections.Generic;

namespace Application.Services.CompanionSrvs.CompanionReservePackageItemSrv.Dto
{
    /// <summary>یک پکیج داخل رزرو با وضعیتش. Status: 1 در انتظار تأیید، 2 تأییدشده، 3 لغوشده.</summary>
    public class CompanionReservePackageItemVDto
    {
        /// <summary>۰ برای رزروهای قدیمی که هنوز ردیفی ندارند (مجازی، همیشه «در انتظار»/«تأییدشده»)</summary>
        public long Id { get; set; }
        public long PackageId { get; set; }
        public string PackageName { get; set; }
        public int PetCount { get; set; }
        public double Price { get; set; }
        public int Status { get; set; }
        public string StatusReason { get; set; }
        public DateTime? StatusChangedDate { get; set; }
        public bool StatusChangedByAdmin { get; set; }
        /// <summary>مبلغی که با لغو این پکیج به کیف پول کاربر برگشته</summary>
        public double RefundAmount { get; set; }
        public DateTime? RefundDate { get; set; }
        public bool AddedAfterPayment { get; set; }
        public DateTime? CreateDate { get; set; }
    }

    public class CompanionReservePackageSummaryVDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Cancelled { get; set; }
        /// <summary>جمع مبالغ برگشت‌خورده به کیف پول از لغو پکیج‌ها</summary>
        public double RefundedTotal { get; set; }
    }

    public class CompanionReservePackageItemsVDto
    {
        public long ReserveId { get; set; }
        /// <summary>true = نماینده/ادمین هنوز می‌تواند تأیید/لغو/افزودن کند (رزرو لغو/کامل/نهایی نشده)</summary>
        public bool CanManage { get; set; }
        public List<CompanionReservePackageItemVDto> Items { get; set; } = new List<CompanionReservePackageItemVDto>();
        public CompanionReservePackageSummaryVDto Summary { get; set; } = new CompanionReservePackageSummaryVDto();
    }

    public class CompanionReservePackageStatusDto
    {
        /// <summary>2 = تأیید، 3 = لغو</summary>
        public int Status { get; set; }
        /// <summary>برای لغو الزامی (به کاربر نشان داده می‌شود)</summary>
        public string Reason { get; set; }
    }

    public class CompanionReservePackageAddDto
    {
        public long PackageId { get; set; }
    }

    public class CompanionReservePackageStatusResultVDto
    {
        public CompanionReservePackageItemVDto Item { get; set; }
        public double RefundAmount { get; set; }
        /// <summary>true = آخرین پکیج فعال هم لغو شد و کل رزرو لغو شد</summary>
        public bool ReserveCancelled { get; set; }
        public CompanionReservePackageItemsVDto Items { get; set; }
    }
}
