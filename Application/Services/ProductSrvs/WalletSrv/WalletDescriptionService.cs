using Application.Services.ProductSrvs.WalletSrv.Dto;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.ProductSrvs.WalletSrv
{
    // کدهای نمایشی مرجع‌های تراکنش‌ها را دسته‌ای (یک پرس‌وجو برای هر نوع) می‌خواند و شرح را با WalletDescriptionRules می‌سازد
    public class WalletDescriptionService : IWalletDescriptionService
    {
        private readonly IDataBaseContext _context;

        public WalletDescriptionService(IDataBaseContext context)
        {
            _context = context;
        }

        public void Apply(IReadOnlyCollection<WalletVDto> rows)
        {
            if (rows == null || rows.Count == 0)
                return;

            var inputs = rows.Select(ToInput).ToList();
            var codes = LoadCodes(inputs);
            foreach (var (row, input) in rows.Zip(inputs))
            {
                row.LedgerName = row.Name;
                row.Name = WalletDescriptionRules.Describe(input, codes);
            }
        }

        public string Describe(WalletDto dto)
        {
            var input = ToInput(dto);
            var codes = LoadCodes(new[] { input });
            return WalletDescriptionRules.Describe(input, codes);
        }

        private WalletCodeLookup LoadCodes(IReadOnlyCollection<WalletDescribeInput> inputs)
        {
            var codes = new WalletCodeLookup();

            var paymentIds = Ids(inputs.Select(i => i.PaymentId)
                .Concat(inputs.Select(i => WalletDescriptionRules.TryGetLedgerId(i.Name, WalletDescriptionRules.PaymentRefundPrefix))));
            if (paymentIds.Count > 0)
                foreach (var row in _context.Payments.AsNoTracking().Where(s => paymentIds.Contains(s.Id)).Select(s => new { s.Id, s.PaymentCode }).ToList())
                    codes.Payments[row.Id] = row.PaymentCode;

            var orderIds = inputs.Select(i => i.ProductOrderId)
                .Concat(inputs.Select(i => WalletDescriptionRules.TryGetLedgerValue(i.Name, WalletDescriptionRules.OrderCancelRefundPrefix, out var v) ? v : null))
                .Concat(inputs.Select(i => WalletDescriptionRules.TryGetLedgerValue(i.Name, WalletDescriptionRules.ReferralBonusPrefix, out var v) ? v : null))
                .Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            if (orderIds.Count > 0)
                foreach (var row in _context.ProductOrders.AsNoTracking().Where(s => orderIds.Contains(s.Id)).Select(s => new { s.Id, s.OrderCode }).ToList())
                    codes.Orders[row.Id] = row.OrderCode;

            var orderItemIds = Ids(inputs.Select(i => WalletDescriptionRules.TryGetLedgerId(i.Name, WalletDescriptionRules.OrderItemRefundPrefix)));
            if (orderItemIds.Count > 0)
                foreach (var row in _context.ProductOrderItems.AsNoTracking().Where(s => orderItemIds.Contains(s.Id))
                             .Select(s => new { s.Id, Code = s.ProductOrderStore.ProductOrder.OrderCode }).ToList())
                    codes.OrderItems[row.Id] = row.Code;

            var companionReserveIds = Ids(inputs.Select(i => i.CompanionReserveId)
                .Concat(inputs.Select(i => WalletDescriptionRules.TryGetLedgerId(i.Name, WalletDescriptionRules.CompanionDebtPrefix))));
            if (companionReserveIds.Count > 0)
                foreach (var row in _context.CompanionReserves.AsNoTracking().Where(s => companionReserveIds.Contains(s.Id)).Select(s => new { s.Id, s.ReserveCode }).ToList())
                    codes.CompanionReserves[row.Id] = row.ReserveCode;

            var packageItemIds = Ids(inputs.Select(i => WalletDescriptionRules.TryGetLedgerId(i.Name, WalletDescriptionRules.CompanionPackageRefundPrefix)));
            if (packageItemIds.Count > 0)
                foreach (var row in _context.CompanionReservePackageItems.AsNoTracking().Where(s => packageItemIds.Contains(s.Id))
                             .Select(s => new { s.Id, s.PackageName, s.CompanionReserve.ReserveCode }).ToList())
                    codes.CompanionPackageItems[row.Id] = (row.ReserveCode, row.PackageName);

            var clinicPackageItemIds = Ids(inputs.Select(i => WalletDescriptionRules.TryGetLedgerId(i.Name, WalletDescriptionRules.ClinicPackageRefundPrefix)));
            if (clinicPackageItemIds.Count > 0)
                foreach (var row in _context.ClinicReserveItems.AsNoTracking().Where(s => clinicPackageItemIds.Contains(s.Id))
                             .Select(s => new { s.Id, s.PackageName, s.ClinicReserve.ReserveCode }).ToList())
                    codes.ClinicPackageItems[row.Id] = (row.ReserveCode, row.PackageName);

            var pansionReserveIds = Ids(inputs.Select(i => i.PansionReserveId)
                .Concat(inputs.Select(i => WalletDescriptionRules.TryGetLedgerId(i.Name, WalletDescriptionRules.PansionReserveRefundPrefix))));
            if (pansionReserveIds.Count > 0)
                foreach (var row in _context.PansionReserves.AsNoTracking().Where(s => pansionReserveIds.Contains(s.Id)).Select(s => new { s.Id, s.ReserveCode }).ToList())
                    codes.PansionReserves[row.Id] = row.ReserveCode;

            var groomerReserveIds = Ids(inputs.Select(i => i.GroomerReserveId)
                .Concat(inputs.Select(i => WalletDescriptionRules.TryGetLedgerId(i.Name, WalletDescriptionRules.GroomerReserveRefundPrefix))));
            if (groomerReserveIds.Count > 0)
                foreach (var row in _context.GroomerReserves.AsNoTracking().Where(s => groomerReserveIds.Contains(s.Id)).Select(s => new { s.Id, s.ReserveCode }).ToList())
                    codes.GroomerReserves[row.Id] = row.ReserveCode;
            var clinicReserveIds = Ids(inputs.Select(i => i.ClinicReserveId)
                .Concat(inputs.Select(i => WalletDescriptionRules.TryGetLedgerId(i.Name, WalletDescriptionRules.ClinicReserveRefundPrefix))));
            if (clinicReserveIds.Count > 0)
                foreach (var row in _context.ClinicReserves.AsNoTracking().Where(s => clinicReserveIds.Contains(s.Id)).Select(s => new { s.Id, s.ReserveCode }).ToList())
                    codes.ClinicReserves[row.Id] = row.ReserveCode;
            var coachReserveIds = Ids(inputs.Select(i => i.CoachReserveId)
                .Concat(inputs.Select(i => WalletDescriptionRules.TryGetLedgerId(i.Name, WalletDescriptionRules.CoachReserveRefundPrefix))));
            if (coachReserveIds.Count > 0)
                foreach (var row in _context.CoachReserves.AsNoTracking().Where(s => coachReserveIds.Contains(s.Id)).Select(s => new { s.Id, s.ReserveCode }).ToList())
                    codes.CoachReserves[row.Id] = row.ReserveCode;

            var schoolReserveIds = Ids(inputs.Select(i => i.SchoolReserveId));
            if (schoolReserveIds.Count > 0)
                foreach (var row in _context.SchoolReserves.AsNoTracking().Where(s => schoolReserveIds.Contains(s.Id)).Select(s => new { s.Id, s.ReserveCode }).ToList())
                    codes.SchoolReserves[row.Id] = row.ReserveCode;

            var consultationIds = Ids(inputs.Select(i => i.ConsultationPurchaseId)
                .Concat(inputs.Select(i => WalletDescriptionRules.TryGetLedgerId(i.Name, WalletDescriptionRules.ConsultationRefundPrefix))));
            if (consultationIds.Count > 0)
                foreach (var row in _context.ConsultationPurchases.AsNoTracking().Where(s => consultationIds.Contains(s.Id)).Select(s => new { s.Id, s.PurchaseCode }).ToList())
                    codes.Consultations[row.Id] = row.PurchaseCode;

            return codes;
        }

        private static List<long> Ids(IEnumerable<long?> ids) =>
            ids.Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();

        private static WalletDescribeInput ToInput(WalletVDto row) => new()
        {
            Name = row.Name,
            IsIncrease = row.IsIncrease,
            PaymentId = row.PaymentId,
            ProductOrderId = row.ProductOrderId,
            CompanionReserveId = row.CompanionReserveId,
            PansionReserveId = row.PansionReserveId,
            TripId = row.TripId,
            CargoId = row.CargoId,
            CompanionInsurancePackageSaleId = row.CompanionInsurancePackageSaleId,
            PastilAiSubscriptionId = row.PastilAiSubscriptionId,
            SchoolReserveId = row.SchoolReserveId,
            GroomerReserveId = row.GroomerReserveId,
            ClinicReserveId = row.ClinicReserveId,
            CoachReserveId = row.CoachReserveId,
            ConsultationPurchaseId = row.ConsultationPurchaseId,
        };

        private static WalletDescribeInput ToInput(WalletDto dto) => new()
        {
            Name = dto.Name,
            IsIncrease = dto.IsIncrease,
            PaymentId = dto.PaymentId,
            ProductOrderId = dto.ProductOrderId,
            CompanionReserveId = dto.CompanionReserveId,
            PansionReserveId = dto.PansionReserveId,
            TripId = dto.TripId,
            CargoId = dto.CargoId,
            CompanionInsurancePackageSaleId = dto.CompanionInsurancePackageSaleId,
            PastilAiSubscriptionId = dto.PastilAiSubscriptionId,
            SchoolReserveId = dto.SchoolReserveId,
            GroomerReserveId = dto.GroomerReserveId,
            ClinicReserveId = dto.ClinicReserveId,
            CoachReserveId = dto.CoachReserveId,
            ConsultationPurchaseId = dto.ConsultationPurchaseId,
        };
    }
}
