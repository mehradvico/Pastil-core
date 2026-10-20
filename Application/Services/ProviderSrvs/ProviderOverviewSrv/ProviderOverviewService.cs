using Application.Common.Dto.Result;
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

            result.Venues.AddRange(await GetCompanionVenuesAsync(providerId, data.Venues));
            result.Venues.AddRange(await GetStoreVenuesAsync(providerId, data.Venues));
            return new BaseResultDto<ProviderOverviewVDto>(true, result);
        }

        private static ProviderFinanceSummaryVDto Summarize(ProviderFinanceDetailVDto data)
        {
            var pools = new[] { data.CompanionPool, data.StorePool };
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
            return await _context.Settlements.AsNoTracking()
                .Where(s => s.ProviderId == providerId)
                .OrderByDescending(s => s.Id)
                .Take(RecentSettlementCount)
                .Select(s => new ProviderSettlementVDto
                {
                    Id = s.Id,
                    Kind = s.Kind == SettlementKindEnum.Store ? StoreKind : CompanionKind,
                    PaidPrice = s.PaidPrice,
                    ItemCount = s.ItemCount,
                    TrackingCode = s.TrackingCode,
                    CreateDate = s.CreateDate,
                })
                .ToListAsync();
        }

        private async Task<List<ProviderVenueOverviewVDto>> GetCompanionVenuesAsync(long providerId, IReadOnlyCollection<ProviderFinanceVenueVDto> finance)
        {
            var companions = await _context.Companions.AsNoTracking()
                .Where(x => x.ProviderId == providerId && !x.Deleted)
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
                .Where(s => ids.Contains(s.CompanionId) && !s.Deleted)
                .GroupBy(s => s.CompanionId)
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
