using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Application.Services.FinanceSrvs.FinanceReportSrv.FinanceReportRules;

namespace Application.Services.FinanceSrvs.FinanceReportSrv
{
    public class FinanceReportInputDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public interface IFinanceReportService
    {
        Task<BaseResultDto<Summary>> GetAsync(FinanceReportInputDto dto);
    }

    // گزارش مالی یکپارچه: همه‌ی پولی که کاربر پرداخت کرده (درگاه + کیف پول) به تفکیک منبع، همراه سهم سایت و شریک.
    // منبع حقیقت: جدول Payments (درگاه‌های موفق) و دفتر Wallets (برداشت‌ها و اعتبارها).
    public class FinanceReportService : IFinanceReportService
    {
        private readonly IDataBaseContext _context;

        public FinanceReportService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto<Summary>> GetAsync(FinanceReportInputDto dto)
        {
            try
            {
                dto ??= new FinanceReportInputDto();
                var from = dto.FromDate?.Date;
                var to = dto.ToDate?.Date.AddDays(1);

                var payments = _context.Payments.AsNoTracking().Where(p => p.IsSuccess == true);
                var wallets = _context.Wallets.AsNoTracking().Where(w => !w.Deleted && !w.Painding);
                if (from.HasValue) { payments = payments.Where(p => p.CreateDate >= from.Value); wallets = wallets.Where(w => w.CreateDate >= from.Value); }
                if (to.HasValue) { payments = payments.Where(p => p.CreateDate < to.Value); wallets = wallets.Where(w => w.CreateDate < to.Value); }

                // درگاه: به تفکیک منبع
                var gateway = (await payments
                    .GroupBy(p => p.CallBackTypeLabel)
                    .Select(g => new { Label = g.Key, Count = g.Count(), Amount = g.Sum(x => x.Amount) })
                    .ToListAsync())
                    .GroupBy(x => NormalizeSource(x.Label))
                    .ToDictionary(g => g.Key ?? "", g => new { Count = g.Sum(x => x.Count), Amount = g.Sum(x => x.Amount) });

                var debits = wallets.Where(w => !w.IsIncrease);
                var walletSpent = new Dictionary<string, double>
                {
                    ["ProductOrder"] = await debits.Where(w => w.ProductOrderId != null).SumAsync(w => (double?)w.Amount) ?? 0,
                    ["CompanionReserve"] = await debits.Where(w => w.CompanionReserveId != null).SumAsync(w => (double?)w.Amount) ?? 0,
                    ["PansionReserve"] = await debits.Where(w => w.PansionReserveId != null).SumAsync(w => (double?)w.Amount) ?? 0,
                    ["SchoolReserve"] = await debits.Where(w => w.SchoolReserveId != null).SumAsync(w => (double?)w.Amount) ?? 0,
                    ["ConsultationPurchase"] = await debits.Where(w => w.ConsultationPurchaseId != null).SumAsync(w => (double?)w.Amount) ?? 0,
                    ["Trip"] = await debits.Where(w => w.TripId != null).SumAsync(w => (double?)w.Amount) ?? 0,
                    ["Cargo"] = await debits.Where(w => w.CargoId != null).SumAsync(w => (double?)w.Amount) ?? 0,
                    ["Insurance"] = await debits.Where(w => w.CompanionInsurancePackageSaleId != null).SumAsync(w => (double?)w.Amount) ?? 0,
                    ["PastilAI"] = await debits.Where(w => w.PastilAiSubscriptionId != null).SumAsync(w => (double?)w.Amount) ?? 0,
                };

                // سهم سایت هر منبع (از ستون‌های SiteShare ثبت‌شده‌ی رکوردهای معتبر)

                var orders = _context.ProductOrders.AsNoTracking().Where(o => !o.Deleted && o.IsPaid);
                var reserves = _context.CompanionReserves.AsNoTracking().Where(r => r.IsReserved && !r.IsCancel);
                var pansions = _context.PansionReserves.AsNoTracking().Where(r => r.IsReserved && !r.IsCancel);
                var schools = _context.SchoolReserves.AsNoTracking().Where(r => r.IsReserved && !r.IsCancel);
                var paidStatuses = new[] { (int)ConsultationPurchaseStatusEnum.Paid, (int)ConsultationPurchaseStatusEnum.Active, (int)ConsultationPurchaseStatusEnum.Completed };
                var consultations = _context.ConsultationPurchases.AsNoTracking().Where(c => paidStatuses.Contains(c.Status));
                var trips = _context.Trips.AsNoTracking().Where(t => t.IsPaid);
                if (from.HasValue)
                {
                    orders = orders.Where(x => x.CreateDate >= from.Value); reserves = reserves.Where(x => x.CreateDate >= from.Value);
                    pansions = pansions.Where(x => x.CreateDate >= from.Value); schools = schools.Where(x => x.CreateDate >= from.Value);
                    consultations = consultations.Where(x => x.CreateDate >= from.Value); trips = trips.Where(x => x.CreateDate >= from.Value);
                }
                if (to.HasValue)
                {
                    orders = orders.Where(x => x.CreateDate < to.Value); reserves = reserves.Where(x => x.CreateDate < to.Value);
                    pansions = pansions.Where(x => x.CreateDate < to.Value); schools = schools.Where(x => x.CreateDate < to.Value);
                    consultations = consultations.Where(x => x.CreateDate < to.Value); trips = trips.Where(x => x.CreateDate < to.Value);
                }

                var siteShare = new Dictionary<string, double>
                {
                    ["ProductOrder"] = await orders.SumAsync(x => (double?)x.SiteShare) ?? 0,
                    ["CompanionReserve"] = await reserves.SumAsync(x => (double?)x.SiteShare) ?? 0,
                    ["PansionReserve"] = await pansions.SumAsync(x => (double?)x.SiteShare) ?? 0,
                    ["SchoolReserve"] = await schools.SumAsync(x => (double?)x.SiteShare) ?? 0,
                    ["ConsultationPurchase"] = await consultations.SumAsync(x => (double?)x.SiteShare) ?? 0,
                    ["Trip"] = await trips.SumAsync(x => (double?)x.SiteShare) ?? 0,
                };

                var rows = new List<SourceRow>();
                foreach (var key in SourceKeys)
                {
                    gateway.TryGetValue(key, out var g);
                    var gatewayPaid = g?.Amount ?? 0;
                    var spent = walletSpent[key];
                    var received = Received(gatewayPaid, spent);
                    // بیمه، بار و اشتراک هوش مصنوعی مستقیم درآمد سایت‌اند
                    var site = siteShare.TryGetValue(key, out var s) ? s : received;
                    rows.Add(new SourceRow
                    {
                        Source = key,
                        PaymentCount = g?.Count ?? 0,
                        GatewayPaid = gatewayPaid,
                        WalletSpent = spent,
                        Received = received,
                        SiteShare = Math.Min(site, received),
                        PartnerShare = PartnerShare(received, site)
                    });
                }

                // اعتبارهای کیف پول بدون پرداخت درگاهی = بازپرداخت/تعدیل؛ شارژ درگاهی جدا
                var refunded = await wallets.Where(w => w.IsIncrease && w.PaymentId == null).SumAsync(w => (double?)w.Amount) ?? 0;
                var topUps = gateway.TryGetValue(PaymentCallbackTypeEnum.Wallet.ToString(), out var t) ? t.Amount : 0;

                return new BaseResultDto<Summary>(true, Build(rows, refunded, topUps));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<Summary>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }
    }
}
