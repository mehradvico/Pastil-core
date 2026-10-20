using System;
using System.Collections.Generic;

namespace Application.Services.ProductSrvs.WalletSrv
{
    // ورودی ساخت «شرح تراکنش» کیف پول؛ هم از موجودیت Wallet و هم از WalletVDto/WalletDto پر می‌شود
    public sealed class WalletDescribeInput
    {
        public string Name { get; set; }
        public bool IsIncrease { get; set; }
        public long? PaymentId { get; set; }
        public string ProductOrderId { get; set; }
        public long? CompanionReserveId { get; set; }
        public long? PansionReserveId { get; set; }
        public long? TripId { get; set; }
        public long? CargoId { get; set; }
        public long? CompanionInsurancePackageSaleId { get; set; }
        public long? PastilAiSubscriptionId { get; set; }
        public long? SchoolReserveId { get; set; }
        public long? GroomerReserveId { get; set; }
        public long? ClinicReserveId { get; set; }
        public long? CoachReserveId { get; set; }
        public long? ConsultationPurchaseId { get; set; }

        public bool HasReference =>
            !string.IsNullOrWhiteSpace(ProductOrderId) || CompanionReserveId.HasValue || PansionReserveId.HasValue ||
            TripId.HasValue || CargoId.HasValue || CompanionInsurancePackageSaleId.HasValue ||
            PastilAiSubscriptionId.HasValue || SchoolReserveId.HasValue || GroomerReserveId.HasValue || ClinicReserveId.HasValue || CoachReserveId.HasValue || ConsultationPurchaseId.HasValue;
    }

    // کدهای قابل نمایش (RSV-... / ORD-... / PAY-...) که سرویس به‌صورت دسته‌ای از دیتابیس می‌خواند
    public sealed class WalletCodeLookup
    {
        public Dictionary<long, string> Payments { get; } = new();
        public Dictionary<string, string> Orders { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<long, string> OrderItems { get; } = new();
        public Dictionary<long, string> CompanionReserves { get; } = new();
        public Dictionary<long, (string ReserveCode, string PackageName)> CompanionPackageItems { get; } = new();
        public Dictionary<long, (string ReserveCode, string PackageName)> ClinicPackageItems { get; } = new();
        public Dictionary<long, string> PansionReserves { get; } = new();
        public Dictionary<long, string> SchoolReserves { get; } = new();
        public Dictionary<long, string> GroomerReserves { get; } = new();
        public Dictionary<long, string> ClinicReserves { get; } = new();
        public Dictionary<long, string> CoachReserves { get; } = new();
        public Dictionary<long, string> Consultations { get; } = new();
    }

    // شرح تراکنش کیف پول هرگز خالی نیست و می‌گوید پول برای چه آمد/رفت (شارژ کاربر، شارژ دستی ادمین، پرداخت رزرو RSV-…،
    // لغو رزرو و بازگشت وجه، ...). Name بعضی ردیف‌ها کلید ضدتکرار دفتر است (PaymentRefund:12 و ...) و نباید عوض شود؛
    // این کلیدها فقط موقع نمایش به متن فارسی ترجمه می‌شوند.
    public static class WalletDescriptionRules
    {
        public const string PaymentRefundPrefix = "PaymentRefund:";
        public const string PansionReserveRefundPrefix = "PansionReserveRefund:";
        public const string GroomerReserveRefundPrefix = "GroomerReserveRefund:";
        public const string ClinicReserveRefundPrefix = "ClinicReserveRefund:";
        public const string CoachReserveRefundPrefix = "CoachReserveRefund:";
        public const string ConsultationRefundPrefix = "ConsultationRefund:";
        public const string CompanionPackageRefundPrefix = "CompanionPackageRefund:";
        public const string ClinicPackageRefundPrefix = "ClinicPackageRefund:";
        public const string OrderCancelRefundPrefix = "OrderCancelRefund:";
        public const string OrderItemRefundPrefix = "OrderItemRefund:";
        public const string ReferralBonusPrefix = "ReferralBonus:";
        public const string AdminManualPrefix = "AdminManual:";
        public const string CompanionDebtPrefix = "CompanionDebt:";
        public const string TripCancelRefundPrefix = "TripCancelRefund:";

        public static string AdminManualName(string note) => AdminManualPrefix + (note ?? string.Empty).Trim();
        public static string CompanionDebtName(long companionReserveId) => CompanionDebtPrefix + companionReserveId;
        public static string TripCancelRefundName(long tripId) => TripCancelRefundPrefix + tripId;

        public static bool TryGetLedgerValue(string name, string prefix, out string value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(name) || !name.StartsWith(prefix, StringComparison.Ordinal))
                return false;
            value = name.Substring(prefix.Length);
            return true;
        }

        public static long? TryGetLedgerId(string name, string prefix)
        {
            if (!TryGetLedgerValue(name, prefix, out var value))
                return null;
            var idPart = value.Split(':')[0];
            return long.TryParse(idPart, out var id) ? id : null;
        }

        public static bool IsLedgerKey(string name) =>
            TryGetLedgerValue(name, PaymentRefundPrefix, out _) || TryGetLedgerValue(name, PansionReserveRefundPrefix, out _) || TryGetLedgerValue(name, GroomerReserveRefundPrefix, out _) || TryGetLedgerValue(name, ClinicReserveRefundPrefix, out _) || TryGetLedgerValue(name, CoachReserveRefundPrefix, out _) ||
            TryGetLedgerValue(name, ConsultationRefundPrefix, out _) || TryGetLedgerValue(name, CompanionPackageRefundPrefix, out _) || TryGetLedgerValue(name, ClinicPackageRefundPrefix, out _) ||
            TryGetLedgerValue(name, OrderCancelRefundPrefix, out _) || TryGetLedgerValue(name, OrderItemRefundPrefix, out _) ||
            TryGetLedgerValue(name, ReferralBonusPrefix, out _) || TryGetLedgerValue(name, AdminManualPrefix, out _) ||
            TryGetLedgerValue(name, CompanionDebtPrefix, out _) || TryGetLedgerValue(name, TripCancelRefundPrefix, out _);

        public static string Describe(WalletDescribeInput wallet, WalletCodeLookup codes)
        {
            codes ??= new WalletCodeLookup();
            var name = wallet.Name?.Trim();

            var ledger = DescribeLedger(wallet, name, codes);
            if (ledger != null)
                return ledger;

            // Name آزاد (غیر از «پرداخت آنلاین» قدیمی): ردیف‌های مرجع‌دار (مثل متن قدیمی بدهی خدمت) همان متن را نشان می‌دهند؛
            // ردیف بی‌مرجع و بی‌پرداخت فقط از فرم شارژ/کسر دستی پنل ساخته می‌شد
            if (!string.IsNullOrWhiteSpace(name) && name != Resource.Lang.OnlinePayment)
            {
                if (wallet.HasReference || wallet.PaymentId.HasValue)
                    return name;
                return AdminText(wallet.IsIncrease, name);
            }

            var target = DescribeTarget(wallet, codes);
            if (target != null)
                return string.Format(wallet.IsIncrease ? Resource.Pattern.WalletReasonRefundFor : Resource.Pattern.WalletReasonPayFor, target);

            if (wallet.PaymentId.HasValue)
            {
                var paymentCode = Code(codes.Payments, wallet.PaymentId.Value);
                return wallet.IsIncrease
                    ? string.Format(Resource.Pattern.WalletReasonUserCharge, paymentCode)
                    : string.Format(Resource.Pattern.WalletReasonPayFor, string.Format(Resource.Pattern.WalletTargetPayment, paymentCode));
            }

            return wallet.IsIncrease ? Resource.Notification.WalletReasonGenericIncrease : Resource.Notification.WalletReasonGenericDecrease;
        }

        private static string DescribeLedger(WalletDescribeInput wallet, string name, WalletCodeLookup codes)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            if (TryGetLedgerValue(name, AdminManualPrefix, out var note))
                return AdminText(wallet.IsIncrease, note);

            var id = TryGetLedgerId(name, PaymentRefundPrefix);
            if (id.HasValue)
                return string.Format(Resource.Pattern.WalletReasonRefundFor,
                    string.Format(Resource.Pattern.WalletTargetPayment, Code(codes.Payments, id.Value)));

            id = TryGetLedgerId(name, GroomerReserveRefundPrefix);
            if (id.HasValue)
                return string.Format(Resource.Pattern.WalletReasonCancelRefundFor,
                    string.Format(Resource.Pattern.WalletTargetGroomerReserve, Code(codes.GroomerReserves, id.Value)));
            id = TryGetLedgerId(name, ClinicReserveRefundPrefix);
            if (id.HasValue)
                return string.Format(Resource.Pattern.WalletReasonCancelRefundFor,
                    string.Format(Resource.Pattern.WalletTargetClinicReserve, Code(codes.ClinicReserves, id.Value)));
            id = TryGetLedgerId(name, CoachReserveRefundPrefix);
            if (id.HasValue)
                return string.Format(Resource.Pattern.WalletReasonCancelRefundFor,
                    string.Format(Resource.Pattern.WalletTargetCoachReserve, Code(codes.CoachReserves, id.Value)));

            id = TryGetLedgerId(name, PansionReserveRefundPrefix);
            if (id.HasValue)
                return string.Format(Resource.Pattern.WalletReasonCancelRefundFor,
                    string.Format(Resource.Pattern.WalletTargetPansionReserve, Code(codes.PansionReserves, id.Value)));

            id = TryGetLedgerId(name, ConsultationRefundPrefix);
            if (id.HasValue)
                return string.Format(Resource.Pattern.WalletReasonCancelRefundFor,
                    string.Format(Resource.Pattern.WalletTargetConsultation, Code(codes.Consultations, id.Value)));

            id = TryGetLedgerId(name, CompanionPackageRefundPrefix);
            if (id.HasValue)
            {
                (string ReserveCode, string PackageName) item = codes.CompanionPackageItems.TryGetValue(id.Value, out var found)
                    ? found
                    : (null, null);
                return string.Format(Resource.Pattern.WalletReasonCompanionPackageRefund,
                    string.Format(Resource.Pattern.WalletTargetCompanionReserve, string.IsNullOrWhiteSpace(item.ReserveCode) ? "#" + id.Value : item.ReserveCode),
                    string.IsNullOrWhiteSpace(item.PackageName) ? "#" + id.Value : item.PackageName);
            }

            id = TryGetLedgerId(name, ClinicPackageRefundPrefix);
            if (id.HasValue)
            {
                (string ReserveCode, string PackageName) clinicItem = codes.ClinicPackageItems.TryGetValue(id.Value, out var foundClinic)
                    ? foundClinic
                    : (null, null);
                return string.Format(Resource.Pattern.WalletReasonCompanionPackageRefund,
                    string.Format(Resource.Pattern.WalletTargetClinicReserve, string.IsNullOrWhiteSpace(clinicItem.ReserveCode) ? "#" + id.Value : clinicItem.ReserveCode),
                    string.IsNullOrWhiteSpace(clinicItem.PackageName) ? "#" + id.Value : clinicItem.PackageName);
            }

            if (TryGetLedgerValue(name, OrderCancelRefundPrefix, out var orderId))
                return string.Format(Resource.Pattern.WalletReasonCancelRefundFor,
                    string.Format(Resource.Pattern.WalletTargetProductOrder, OrderCode(codes, orderId)));

            id = TryGetLedgerId(name, OrderItemRefundPrefix);
            if (id.HasValue)
                return string.Format(Resource.Pattern.WalletReasonOrderItemRefund,
                    string.Format(Resource.Pattern.WalletTargetProductOrder,
                        codes.OrderItems.TryGetValue(id.Value, out var orderCode) && !string.IsNullOrWhiteSpace(orderCode) ? orderCode : "#" + id.Value));

            if (TryGetLedgerValue(name, ReferralBonusPrefix, out var bonusOrderId))
                return string.Format(Resource.Pattern.WalletReasonReferralBonus,
                    string.Format(Resource.Pattern.WalletTargetProductOrder, OrderCode(codes, bonusOrderId)));

            id = TryGetLedgerId(name, TripCancelRefundPrefix);
            if (id.HasValue)
                return string.Format(Resource.Pattern.WalletReasonCancelRefundFor,
                    string.Format(Resource.Pattern.WalletTargetTrip, id.Value));

            id = TryGetLedgerId(name, CompanionDebtPrefix);
            if (id.HasValue)
                return string.Format(Resource.Pattern.WalletReasonCompanionDebt,
                    string.Format(Resource.Pattern.WalletTargetCompanionReserve, Code(codes.CompanionReserves, id.Value)));

            return null;
        }

        private static string DescribeTarget(WalletDescribeInput wallet, WalletCodeLookup codes)
        {
            if (!string.IsNullOrWhiteSpace(wallet.ProductOrderId))
                return string.Format(Resource.Pattern.WalletTargetProductOrder, OrderCode(codes, wallet.ProductOrderId));
            if (wallet.CompanionReserveId.HasValue)
                return string.Format(Resource.Pattern.WalletTargetCompanionReserve, Code(codes.CompanionReserves, wallet.CompanionReserveId.Value));
            if (wallet.PansionReserveId.HasValue)
                return string.Format(Resource.Pattern.WalletTargetPansionReserve, Code(codes.PansionReserves, wallet.PansionReserveId.Value));
            if (wallet.GroomerReserveId.HasValue)
                return string.Format(Resource.Pattern.WalletTargetGroomerReserve, Code(codes.GroomerReserves, wallet.GroomerReserveId.Value));
            if (wallet.ClinicReserveId.HasValue)
                return string.Format(Resource.Pattern.WalletTargetClinicReserve, Code(codes.ClinicReserves, wallet.ClinicReserveId.Value));
            if (wallet.CoachReserveId.HasValue)
                return string.Format(Resource.Pattern.WalletTargetCoachReserve, Code(codes.CoachReserves, wallet.CoachReserveId.Value));
            if (wallet.SchoolReserveId.HasValue)
                return string.Format(Resource.Pattern.WalletTargetSchoolReserve, Code(codes.SchoolReserves, wallet.SchoolReserveId.Value));
            if (wallet.ConsultationPurchaseId.HasValue)
                return string.Format(Resource.Pattern.WalletTargetConsultation, Code(codes.Consultations, wallet.ConsultationPurchaseId.Value));
            if (wallet.TripId.HasValue)
                return string.Format(Resource.Pattern.WalletTargetTrip, wallet.TripId.Value);
            if (wallet.CargoId.HasValue)
                return string.Format(Resource.Pattern.WalletTargetCargo, wallet.CargoId.Value);
            if (wallet.CompanionInsurancePackageSaleId.HasValue)
                return string.Format(Resource.Pattern.WalletTargetInsurance, wallet.CompanionInsurancePackageSaleId.Value);
            if (wallet.PastilAiSubscriptionId.HasValue)
                return string.Format(Resource.Pattern.WalletTargetPastilAi, wallet.PastilAiSubscriptionId.Value);
            return null;
        }

        private static string AdminText(bool isIncrease, string note)
        {
            note = note?.Trim();
            if (string.IsNullOrWhiteSpace(note))
                return isIncrease ? Resource.Notification.WalletReasonAdminCharge : Resource.Notification.WalletReasonAdminDebit;
            return string.Format(isIncrease ? Resource.Pattern.WalletReasonAdminChargeNote : Resource.Pattern.WalletReasonAdminDebitNote, note);
        }

        private static string Code(Dictionary<long, string> codes, long id) =>
            codes.TryGetValue(id, out var code) && !string.IsNullOrWhiteSpace(code) ? code : "#" + id;

        private static string OrderCode(WalletCodeLookup codes, string orderId) =>
            codes.Orders.TryGetValue(orderId ?? string.Empty, out var code) && !string.IsNullOrWhiteSpace(code) ? code : orderId;
    }
}
