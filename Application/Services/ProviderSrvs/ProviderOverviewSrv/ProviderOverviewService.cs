using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionSrv;
using Application.Services.ProductSrvs.WalletSrv.IFace;
using Application.Services.ProviderSrvs.ProviderFinanceSrv.Dto;
using Application.Services.ProviderSrvs.ProviderFinanceSrv.Iface;
using Application.Services.ProviderSrvs.ProviderOverviewSrv.Dto;
using Application.Services.ProviderSrvs.ProviderOverviewSrv.Iface;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProviderSrvs.ProviderOverviewSrv
{
    /// <summary>
    /// خلاصه‌ی صفحه‌ی جزئیات ارائه‌دهنده. فقط ترکیب می‌کند: همه‌ی عددهای مالی از IProviderFinanceService می‌آیند
    /// (تنها منبع حسابداری ارائه‌دهنده)؛ این‌جا فقط اطلاعات توصیفی نمایندگی‌ها/فروشگاه‌ها و کیف پول اضافه می‌شود.
    /// </summary>
    public class ProviderOverviewService : IProviderOverviewService
    {
        private const string CompanionKind = "Companion";
        private const string StoreKind = "Store";
        private const string GroomerKind = "Groomer";
        private const string ClinicKind = "Clinic";
        private const string CoachKind = "Coach";
        private const string SchoolKind = "School";
        private const string PansionKind = "Pansion";

        private static string KindName(SettlementKindEnum kind) => kind switch
        {
            SettlementKindEnum.Store => StoreKind,
            SettlementKindEnum.Groomer => GroomerKind,
            SettlementKindEnum.Clinic => ClinicKind,
            SettlementKindEnum.Coach => CoachKind,
            SettlementKindEnum.School => SchoolKind,
            SettlementKindEnum.Pansion => PansionKind,
            _ => CompanionKind,
        };
        private const int RecentSettlementCount = 10;

        private readonly IDataBaseContext _context;
        private readonly IProviderFinanceService _finance;
        private readonly IWalletService _wallet;

        public ProviderOverviewService(IDataBaseContext context, IProviderFinanceService finance, IWalletService wallet)
        {
            _context = context;
            _finance = finance;
            _wallet = wallet;
        }

        public async Task<BaseResultDto<ProviderOverviewVDto>> GetAsync(long providerId)
        {
            var detail = await _finance.GetDetailAsync(providerId);
            if (!detail.IsSuccess || detail.Data == null)
                return new BaseResultDto<ProviderOverviewVDto>(false, Resource.Notification.ProviderNotFound, null);

            var data = detail.Data;
            var result = new ProviderOverviewVDto
            {
                WalletAmount = await _wallet.GetAmountValueAsync(data.UserId),
                BankCards = data.Cards.Select(x => new ProviderBankCardVDto
                {
                    Id = x.Id,
                    BankName = x.BankName,
                    CardNumber = x.CardNumber,
                    ShebaNumber = x.ShebaNumber,
                    CardHolderName = x.CardHolderName,
                    Approved = x.Approved,
                }).ToList(),
                Finance = Summarize(data),
                RecentSettlements = await GetRecentSettlementsAsync(providerId),
            };

            // همه‌ی مراکز ارائه‌دهنده (همان ترتیب و همان قاعده‌ی نمایش هاب نمایندگی‌ها)
            result.Venues.AddRange(await GetCompanionVenuesAsync(providerId, data.Venues));
            result.Venues.AddRange(await GetStoreVenuesAsync(providerId, data.Venues));
            result.Venues.AddRange(await GetSimpleVenuesAsync(GroomerKind, SettlementKindEnum.Groomer, data.Venues,
                _context.Groomers.AsNoTracking().Where(x => x.ProviderId == providerId && !x.Deleted).OrderBy(x => x.Id)
                    .Select(x => new ProviderVenueOverviewVDto { Id = x.Id, Name = x.Name, Active = x.Active, Approved = x.Approved, RateAvg = x.RateAvg, RateCount = x.RateCount })));
            result.Venues.AddRange(await GetSimpleVenuesAsync(ClinicKind, SettlementKindEnum.Clinic, data.Venues,
                _context.Clinics.AsNoTracking().Where(x => x.ProviderId == providerId && !x.Deleted).OrderBy(x => x.Id)
                    .Select(x => new ProviderVenueOverviewVDto { Id = x.Id, Name = x.Name, Active = x.Active, Approved = x.Approved, RateAvg = x.RateAvg, RateCount = x.RateCount })));
            result.Venues.AddRange(await GetSimpleVenuesAsync(CoachKind, SettlementKindEnum.Coach, data.Venues,
                _context.Coaches.AsNoTracking().Where(x => x.ProviderId == providerId && !x.Deleted).OrderBy(x => x.Id)
                    .Select(x => new ProviderVenueOverviewVDto { Id = x.Id, Name = x.Name, Active = x.Active, Approved = x.Approved, RateAvg = x.RateAvg, RateCount = x.RateCount })));
            result.Venues.AddRange(await GetSimpleVenuesAsync(SchoolKind, SettlementKindEnum.School, data.Venues,
                _context.Schools.AsNoTracking().Where(x => x.ProviderId == providerId && !x.Deleted).OrderBy(x => x.Id)
                    .Select(x => new ProviderVenueOverviewVDto { Id = x.Id, Name = x.Name, Active = x.Active, Approved = x.Approve, RateAvg = x.RateAvg, RateCount = x.RateCount })));
            result.Venues.AddRange(await GetSimpleVenuesAsync(PansionKind, SettlementKindEnum.Pansion, data.Venues,
                _context.Pansions.AsNoTracking().Where(x => x.ProviderId == providerId && !x.Deleted).OrderBy(x => x.Id)
                    .Select(x => new ProviderVenueOverviewVDto { Id = x.Id, Name = x.Name, Active = x.Active, Approved = x.Approve, RateAvg = x.RateAvg, RateCount = x.RateCount })));
            return new BaseResultDto<ProviderOverviewVDto>(true, result);
        }

        private static ProviderFinanceSummaryVDto Summarize(ProviderFinanceDetailVDto data)
        {
            // همه‌ی استخرها؛ قبلاً فقط نمایندگی + فروشگاه جمع می‌شد و پول آرایشگاه/کلینیک/مربی/مدرسه/پانسیون در خلاصه دیده نمی‌شد
            var pools = new[] { data.CompanionPool, data.StorePool, data.GroomerPool, data.ClinicPool, data.CoachPool, data.SchoolPool, data.PansionPool }
                .Where(p => p != null).ToList();
            return new ProviderFinanceSummaryVDto
            {
                ItemCount = pools.Sum(p => p.ItemCount),
                ProviderShare = pools.Sum(p => p.ProviderShare),
                SiteShare = pools.Sum(p => p.SiteShare),
                SettledShare = pools.Sum(p => p.SettledShare),
                UnsettledShare = pools.Sum(p => p.UnsettledShare),
                PaidOut = pools.Sum(p => p.PaidOut),
                SettlementCount = pools.Sum(p => p.SettlementCount),
                LastSettlementDate = pools.Max(p => p.LastSettlementDate),
            };
        }

        private async Task<List<ProviderSettlementVDto>> GetRecentSettlementsAsync(long providerId)
        {
            var rows = await _context.Settlements.AsNoTracking()
                .Where(s => s.ProviderId == providerId)
                .OrderByDescending(s => s.Id)
                .Take(RecentSettlementCount)
                .Select(s => new { s.Id, s.Kind, s.PaidPrice, s.ItemCount, s.TrackingCode, s.CreateDate })
                .ToListAsync();
            return rows.Select(s => new ProviderSettlementVDto
            {
                Id = s.Id,
                Kind = KindName(s.Kind),
                PaidPrice = s.PaidPrice,
                ItemCount = s.ItemCount,
                TrackingCode = s.TrackingCode,
                CreateDate = s.CreateDate,
            }).ToList();
        }

        private async Task<List<ProviderVenueOverviewVDto>> GetCompanionVenuesAsync(long providerId, IReadOnlyCollection<ProviderFinanceVenueVDto> finance)
        {
            // نمایندگی قدیمی که کاملاً به آرایشگاه/کلینیک/مربی منتقل شده یا فقط ظرف پانسیون/مدرسه است، مثل هاب نمایندگی‌ها نمایش داده نمی‌شود
            // (مگر هنوز پول تسویه‌نشده داشته باشد تا هیچ مبلغی پنهان نماند)
            var ownIds = _context.Companions.AsNoTracking().Where(x => x.ProviderId == providerId && !x.Deleted);
            var allIds = await ownIds.Select(x => x.Id).ToListAsync();
            var shownIds = await ownIds
                .Where(CompanionMigrationFilter.NotFullyMigrated(_context))
                .Where(CompanionMigrationFilter.NotOnlyPansionSchoolContainer(_context))
                .Select(x => x.Id)
                .ToListAsync();
            var hiddenIds = allIds.Except(shownIds)
                .Where(id => !finance.Any(f => f.Kind == SettlementKindEnum.Companion && f.Id == id && f.UnsettledShare > 0))
                .ToList();

            var companions = await _context.Companions.AsNoTracking()
                .Where(x => x.ProviderId == providerId && !x.Deleted && !hiddenIds.Contains(x.Id))
                .OrderBy(x => x.Id)
                .Select(x => new ProviderVenueOverviewVDto
                {
                    Kind = CompanionKind,
                    Id = x.Id,
                    Name = x.Name,
                    Active = x.Active,
                    Approved = x.Approved,
                    RateAvg = x.RateAvg,
                    RateCount = x.RateCount,
                    ServiceCount = x.CompanionAssistances.Count(a => !a.Deleted && a.Active),
                    StaffCount = x.CompanionUsers.Count(u => !u.Deleted && u.Active && u.UserAccept == true),
                    PansionCount = x.Pansions.Count(p => !p.Deleted),
                })
                .ToListAsync();

            var ids = companions.Select(x => x.Id).ToList();
            var schoolCounts = await _context.Schools.AsNoTracking()
                .Where(s => s.CompanionId != null && ids.Contains(s.CompanionId.Value) && !s.Deleted)
                .GroupBy(s => s.CompanionId.Value)
                .Select(g => new { CompanionId = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var venue in companions)
            {
                venue.SchoolCount = schoolCounts.FirstOrDefault(x => x.CompanionId == venue.Id)?.Count ?? 0;
                ApplyFinance(venue, finance.FirstOrDefault(f => f.Kind == SettlementKindEnum.Companion && f.Id == venue.Id));
            }
            return companions;
        }

        private async Task<List<ProviderVenueOverviewVDto>> GetStoreVenuesAsync(long providerId, IReadOnlyCollection<ProviderFinanceVenueVDto> finance)
        {
            var stores = await _context.Stores.AsNoTracking()
                .Where(x => x.ProviderId == providerId && !x.Deleted)
                .OrderBy(x => x.Id)
                .Select(x => new ProviderVenueOverviewVDto
                {
                    Kind = StoreKind,
                    Id = x.Id,
                    Name = x.Name,
                    Active = x.Active,
                    Approved = x.Approved,
                    RateAvg = x.RateAvg,
                    RateCount = x.RateCount,
                })
                .ToListAsync();

            foreach (var venue in stores)
                ApplyFinance(venue, finance.FirstOrDefault(f => f.Kind == SettlementKindEnum.Store && f.Id == venue.Id));
            return stores;
        }

        private static async Task<List<ProviderVenueOverviewVDto>> GetSimpleVenuesAsync(string kind, SettlementKindEnum financeKind,
            IReadOnlyCollection<ProviderFinanceVenueVDto> finance, IQueryable<ProviderVenueOverviewVDto> query)
        {
            var venues = await query.ToListAsync();
            foreach (var venue in venues)
            {
                venue.Kind = kind;
                ApplyFinance(venue, finance.FirstOrDefault(f => f.Kind == financeKind && f.Id == venue.Id));
            }
            return venues;
        }

        private static void ApplyFinance(ProviderVenueOverviewVDto venue, ProviderFinanceVenueVDto finance)
        {
            if (finance == null)
                return;
            venue.ItemCount = finance.ItemCount;
            venue.ProviderShare = finance.ProviderShare;
            venue.SiteShare = finance.SiteShare;
            venue.SettledShare = finance.SettledShare;
            venue.UnsettledShare = finance.UnsettledShare;
        }
    }
}
