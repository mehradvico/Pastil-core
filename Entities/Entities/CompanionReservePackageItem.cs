using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities
{
    // وضعیت هر پکیجِ داخل یک رزرو نماینده (تأیید/لغو تکی توسط نماینده یا ادمین). فهرست پکیج‌های رزرو همچنان همان رابطه‌ی
    // چند‌به‌چند قبلی (CompanionReserve.CompanionAssistancePackages) است و دست‌نخورده می‌ماند؛ این جدول فقط وضعیت و مبلغ
    // هر پکیج را کنارش نگه می‌دارد. رزروهای قدیمی ردیف ندارند و هنگام اولین عملیات (یا نمایش مجازی) ساخته می‌شوند.
    // طراحی: backend/Docs/COMPANION_RESERVE_PACKAGE_ITEMS_FA.md
    public class CompanionReservePackageItem : Id_Field
    {
        public long CompanionReserveId { get; set; }
        public long CompanionAssistancePackageId { get; set; }
        // نام پکیج در لحظه‌ی ثبت (برای تاریخچه، اگر بعداً پکیج ویرایش/حذف شد)
        public string PackageName { get; set; }
        public int PetCount { get; set; }
        // قیمت کامل این پکیج برای همه‌ی پت‌های رزرو (قبل از تخفیف)
        public double Price { get; set; }
        // سهم پیش‌پرداخت این پکیج؛ وزن محاسبه‌ی بازپرداخت. پکیجِ اضافه‌شده بعد از پرداخت = ۰ (چیزی پیش‌پرداخت نشده)
        public double PrePaymentPrice { get; set; }
        // CompanionReservePackageItemStatusEnum
        public int Status { get; set; }
        public string StatusReason { get; set; }
        public DateTime? StatusChangedDate { get; set; }
        public long? StatusChangedByUserId { get; set; }
        public bool StatusChangedByAdmin { get; set; }
        // مبلغی که با لغو این پکیج به کیف پول کاربر برگشت
        public double RefundAmount { get; set; }
        public DateTime? RefundDate { get; set; }
        public bool AddedAfterPayment { get; set; }
        public DateTime CreateDate { get; set; }

        public CompanionReserve CompanionReserve { get; set; }
        public CompanionAssistancePackage CompanionAssistancePackage { get; set; }
        public User StatusChangedByUser { get; set; }
    }
}
