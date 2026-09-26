using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class CompanionReserve : Id_Field
    {
        public string ReserveCode { get; set; }
        public long BookerId { get; set; }
        // اگر این رزرو بخشی از یک سبد رزرو چندخدمتی (همان کلینیک) باشد که با یک پرداخت مشترک
        // تسویه می‌شود؛ نال یعنی رزرو مستقل و تک‌خدمتی همیشگی است (بدون تغییر در رفتار قبلی).
        public long? BatchId { get; set; }
        public double PrePaymentPrice { get; set; }
        public double OperatorFinalPrice { get; set; }
        public double OperatorStuffPrice { get; set; }
        public double OperatorWagesPrice { get; set; }
        public bool FromWallet { get; set; }
        public double WalletPrice { get; set; }
        public double PaymentPrice { get; set; }
        public double PackagePrice { get; set; }
        public long? AddressId { get; set; }
        public long CompanionAssistanceId { get; set; }
        public long CompanionAssistanceTypeId { get; set; }
        public long? CompanionAssistanceTimeId { get; set; }
        // زمان نوبت بر اساس ساعت کاری مرکز (جایگزین CompanionAssistanceTimeId برای رزروهای جدید؛
        // آن فیلد فقط برای نمایش تاریخچه‌ی رزروهای قدیمی‌تر که بر اساس زمان‌بندی هر خدمت ثبت شده‌اند نگه داشته شده).
        public long? CompanionTimeId { get; set; }
        public long? CompanionAssistanceUserId { get; set; }
        // روش ارتباط آنلاین انتخابی کاربر برای این رزرو (چت/تماس/ویدیو کال) - نال یعنی این رزرو آنلاین نیست.
        // یادآورهای Push (۱۰ دقیقه قبل و سر زمان DoDate) بر اساس همین فیلد به نماینده ارسال می‌شوند.
        public long? CompanionAssistancePackageOnlineSelectionId { get; set; }
        // زمان شروع و پایان تماس درون‌برنامه‌ای (وقتی روش ارتباط، تماس فوری داخل برنامه باشد) - برای نمایش به ادمین.
        public DateTime? CallStartDate { get; set; }
        public DateTime? CallEndDate { get; set; }
        public bool? IsFemale { get; set; }
        public string BookerDetail { get; set; }
        public string AssistanceDetail { get; set; }
        public bool IsReserved { get; set; }
        public bool IsCancel { get; set; }
        public string CancelDetail { get; set; }
        public long StateId { get; set; }
        public DateTime DoDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DoneDate { get; set; }
        public DateTime? CancelDate { get; set; }


        public long OperatorStateId { get; set; }
        public DateTime? OperatorChangeStateDate { get; set; }
        public string OperatorDetail { get; set; }
        public bool? UserResponse { get; set; }

        // «پرداخت‌نشده»: اپراتور/پزشک هزینه‌ی نهایی خدمت را ثبت کرده ولی کاربر پرداخت نکرده است.
        // بدهی باز = OperatorUnpaid && OperatorDebtPaidDate == null؛ بعد از ۷ روز رزرو جدید برای کاربر قفل می‌شود.
        public bool OperatorUnpaid { get; set; }
        public double OperatorUnpaidAmount { get; set; }
        public DateTime? OperatorUnpaidDate { get; set; }
        public DateTime? OperatorDebtPaidDate { get; set; }
        // true = کاربر از کیف پول در پاستیل پرداخت کرد؛ false = کلینیک دریافت مستقیم را تأیید کرد
        public bool OperatorDebtPaidByWallet { get; set; }


        public double Discount { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }

        public double CompanionShare { get; set; }
        public double SiteShare { get; set; }
        public bool Permitted { get; set; }

        public User Booker { get; set; }
        public CompanionReserveBatch Batch { get; set; }
        public Code CompanionAssistanceType { get; set; }
        public CompanionAssistance CompanionAssistance { get; set; }
        public CompanionAssistanceTime CompanionAssistanceTime { get; set; }
        public CompanionTime CompanionTime { get; set; }
        public CompanionAssistanceUser CompanionAssistanceUser { get; set; }
        public CompanionAssistancePackageOnlineSelection CompanionAssistancePackageOnlineSelection { get; set; }
        public Code State { get; set; }
        public Code OperatorState { get; set; }
        public Address Address { get; set; }
        public Rebate Rebate { get; set; }
        public Wallet Wallet { get; set; }
        public ICollection<CompanionAssistancePackage> CompanionAssistancePackages { get; set; }
        public ICollection<UserPet> UserPets { get; set; }
    }
}
